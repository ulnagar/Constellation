namespace Constellation.Application.Domains.Students.Events.StudentCreatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models.Students.Events;
using Core.Errors;
using Core.Models.Students;
using Core.Models.Students.Errors;
using Core.Models.Students.Repositories;
using Core.Shared;
using Core.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddUserAccount
    : IDomainEventHandler<StudentCreatedDomainEvent>
{
    private readonly IStudentRepository _studentRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public AddUserAccount(
        IStudentRepository studentRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _userManager = userManager;
        _logger = logger.ForContext<StudentCreatedDomainEvent>();
    }

    public async Task Handle(StudentCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        Student student = await _studentRepository.GetById(notification.StudentId, cancellationToken);

        if (student is null)
        {
            _logger
                .ForContext(nameof(StudentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StudentErrors.NotFound(notification.StudentId), true)
                .Warning("Failed to create new Student AppUser");
            return;
        }

        if (student.EmailAddress == EmailAddress.None)
        {
            _logger
                .ForContext(nameof(StudentCreatedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.ValueObjects.EmailAddress.EmailInvalid, true)
                .Warning("Failed to create new Student AppUser");
            return;
        }

        AppUser user = await _userManager.FindByEmailAsync(student.EmailAddress.Email);

        if (user is not null)
        {
            user.IsStudent = true;
            user.StudentId = student.Id;

            IdentityResult update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(StudentCreatedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to update Student AppUser");

                return;
            }

            return;
        }

        user = new()
        {
            UserName = student.EmailAddress.Email,
            Email = student.EmailAddress.Email,
            FirstName = student.Name.PreferredName,
            LastName = student.Name.LastName,
            IsStudent = true,
            StudentId = student.Id
        };

        IdentityResult create = await _userManager.CreateAsync(user);

        if (create.Succeeded)
        {
            _logger
                .ForContext(nameof(StudentCreatedDomainEvent), notification, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(IdentityResult.Errors), create.Errors, true)
                .Warning("Failed to create new Student AppUser");
        }
    }
}