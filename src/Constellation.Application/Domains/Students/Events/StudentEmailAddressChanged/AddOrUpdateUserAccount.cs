namespace Constellation.Application.Domains.Students.Events.StudentEmailAddressChanged;

using Application.Models.Identity.Repositories;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Enums;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Events;
using Constellation.Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddOrUpdateUserAccount
: IDomainEventHandler<StudentEmailAddressChangedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly IIdentityRepository _identityRepository;
    private readonly ILogger _logger;

    public AddOrUpdateUserAccount(
        IStudentRepository studentRepository,
        UserManager<AppUser> userManager,
        IIdentityRepository identityRepository,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _userManager = userManager;
        _identityRepository = identityRepository;
        _logger = logger
            .ForContext<StudentEmailAddressChangedDomainEvent>();
    }

    public async Task Handle(StudentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        Student? student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(StudentEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Failed to update Student AppUser for new Email");
            return;
        }

        Result<EmailAddress> newAddress = EmailAddress.Create(notification.NewAddress);

        if (newAddress.IsFailure)
        {
            _logger
                .ForContext(nameof(StudentEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), newAddress.Error, true)
                .Warning("Failed to update Student AppUser for new Email");

            return;
        }

        AppUser? user = await _userManager.FindByEmailAsync(notification.OldAddress);

        List<AppRole> oldUserRoles = [];

        if (user is not null)
        {
            AppUserLink? link = user.Links
                .FirstOrDefault(link =>
                    !link.IsDeleted &&
                    link.Type == LinkType.Student &&
                    link.LinkId == student.Id.Value);

            if (link is not null)
                link.Delete();

            await _userManager.UpdateAsync(user);

            if (user.Links.Where(link => link.Type == LinkType.Student).All(link => link.IsDeleted))
            {
                List<AppRole> roles = await _identityRepository.GetRolesForUser(user, cancellationToken);

                foreach (AppRole role in roles.Where(role => role.Type == AppRoleType.Student))
                {
                    await _userManager.RemoveFromRoleAsync(user, role.Name);

                    oldUserRoles.Add(role);
                }
            }

            if (user.Links.All(link => link.IsDeleted))
            {
                IdentityResult update = await _userManager.DeleteAsync(user);

                if (!update.Succeeded)
                {
                    _logger
                        .ForContext(nameof(StudentEmailAddressChangedDomainEvent), notification, true)
                        .ForContext(nameof(AppUser), user, true)
                        .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                        .Warning("Failed to update Student AppUser for new Email");
                }
            }
        }

        user = await _userManager.FindByEmailAsync(newAddress.Value.Email);

        if (user is null)
        {
            user = new()
            {
                UserName = student.EmailAddress.Email, Email = student.EmailAddress.Email, Name = student.Name
            };

            IdentityResult create = await _userManager.CreateAsync(user);

            if (!create.Succeeded)
            {
                _logger
                    .ForContext(nameof(StudentEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), create.Errors, true)
                    .Warning("Failed to update Student AppUser for new Email");

                return;
            }
        }

        List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Student).ToList();

        if (links.All(link => link.LinkId != student.Id.Value))
        {
            user.AddStudentLink(student.Id);

            IdentityResult update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(StudentEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to update Student AppUser for new Email");

                return;
            }
        }

        await _userManager.AddToRoleAsync(user, AppRole.Student);

        foreach (AppRole role in oldUserRoles)
            await _userManager.AddToRoleAsync(user, role.Name);
    }
}