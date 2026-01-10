namespace Constellation.Application.Domains.Auth.Commands.RepairSchoolContactUser;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity.Enums;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Errors;
using Core.Models.SchoolContacts.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using Models.Identity;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RepairSchoolContactUserCommandHandler
: ICommandHandler<RepairSchoolContactUserCommand, AppUser>
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public RepairSchoolContactUserCommandHandler(
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger.ForContext<RepairSchoolContactUserCommand>();
    }

    public async Task<Result<AppUser>> Handle(RepairSchoolContactUserCommand request, CancellationToken cancellationToken)
    {
        SchoolContact? contact = await _contactRepository.GetById(request.ContactId, cancellationToken);
        
        if (contact is null)
        {
            _logger
                .ForContext(nameof(RepairSchoolContactUserCommand), request, true)
                .ForContext(nameof(Error), SchoolContactErrors.NotFound(request.ContactId), true)
                .Warning("Could not repair the School Contact User item");

            return Result.Failure<AppUser>(SchoolContactErrors.NotFound(request.ContactId));
        }

        List<SchoolContactRole> roles = contact.Assignments
            .Where(role => !role.IsDeleted)
            .ToList();

        if (roles.Count == 0)
        {
            if (!contact.IsDeleted)
            {
                contact.Delete();

                await _unitOfWork.CompleteAsync(cancellationToken);
            }
                
            _logger
                .ForContext(nameof(RepairSchoolContactUserCommand), request, true)
                .ForContext(nameof(Error), new Error("Authorisation.AppUser.Deleted", "School Contact has no active roles"), true)
                .Warning("Could not repair the School Contact User item");

            return Result.Failure<AppUser>(new Error("Authorisation.AppUser.Deleted", "School Contact has no active roles"));
        }

        AppUser? user = await _userManager.FindByEmailAsync(contact.EmailAddress.Email);

        if (user is null)
        {
            // Create a new user
            AppUser newUser = new()
            {
                UserName = contact.EmailAddress.Email,
                Email = contact.EmailAddress.Email,
                Name = contact.Name
            };

            newUser.AddContactLink(contact.Id);

            IdentityResult createResult = await _userManager.CreateAsync(newUser);

            if (createResult.Succeeded)
            {
                foreach (var role in roles)
                    await _userManager.AddToRoleAsync(newUser, role.Role.Value);
                
                return newUser;
            }

            _logger
                .ForContext(nameof(RepairSchoolContactUserCommand), request, true)
                .ForContext(nameof(AppUser), newUser, true)
                .ForContext(nameof(Error), new Error("Authorisation.AppUser.Create", "Failed to create AppUser"), true)
                .ForContext(nameof(IdentityResult), createResult, true)
                .Warning("Could not repair the School Contact User item");

            return Result.Failure<AppUser>(new Error("Authorisation.AppUser.Create", "Failed to create AppUser"));
        }

        List<AppUserLink> contactLinks = user.Links
            .Where(link =>
                !link.IsDeleted &&
                link.Type == LinkType.Contact)
            .ToList();

        if (contactLinks.Count == 0)
            user.AddContactLink(contact.Id);

        if (contactLinks.All(link => link.LinkId != contact.Id.Value))
            user.AddContactLink(contact.Id);

        IdentityResult updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            _logger
                .ForContext(nameof(RepairSchoolContactUserCommand), request, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(Error), new Error("Authorisation.AppUser.Update", "Failed to update AppUser"), true)
                .ForContext(nameof(IdentityResult), updateResult, true)
                .Warning("Could not repair the School Contact User item");

            return Result.Failure<AppUser>(new Error("Authorisation.AppUser.Update", "Failed to update AppUser"));
        }

        foreach (var role in roles)
            await _userManager.AddToRoleAsync(user, role.Role.Value);

        return user;
    }
}
