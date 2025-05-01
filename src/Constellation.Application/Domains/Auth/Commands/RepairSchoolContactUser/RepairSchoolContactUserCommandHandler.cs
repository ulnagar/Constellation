namespace Constellation.Application.Domains.Auth.Commands.RepairSchoolContactUser;

using Abstractions.Messaging;
using Core.Models.SchoolContacts;
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
        SchoolContact contact = await _contactRepository.GetById(request.ContactId, cancellationToken);
        List<SchoolContactRole> roles = contact.Assignments
            .Where(role => !role.IsDeleted)
            .ToList();

        if (!roles.Any())
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

        AppUser user = await _userManager.FindByEmailAsync(contact.EmailAddress);

        if (user is null)
        {
            // Create a new user
            AppUser newUser = new()
            {
                UserName = contact.EmailAddress,
                Email = contact.EmailAddress,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                IsSchoolContact = true,
                SchoolContactId = contact.Id
            };

            IdentityResult result = await _userManager.CreateAsync(newUser);

            if (result.Succeeded)
                return newUser;
            
            _logger
                .ForContext(nameof(RepairSchoolContactUserCommand), request, true)
                .ForContext(nameof(AppUser), newUser, true)
                .ForContext(nameof(Error), new Error("Authorisation.AppUser.Create", "Failed to create AppUser"), true)
                .ForContext(nameof(IdentityResult), result, true)
                .Warning("Could not repair the School Contact User item");

            return Result.Failure<AppUser>(new Error("Authorisation.AppUser.Create", "Failed to create AppUser"));
        }

        if (user.IsSchoolContact == false)
        {
            // Link existing user to contact
            user.IsSchoolContact = true;
            user.SchoolContactId = contact.Id;

            IdentityResult result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
                return user;

            _logger
                .ForContext(nameof(RepairSchoolContactUserCommand), request, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(Error), new Error("Authorisation.AppUser.Update", "Failed to update AppUser"), true)
                .ForContext(nameof(IdentityResult), result, true)
                .Warning("Could not repair the School Contact User item");

            return Result.Failure<AppUser>(new Error("Authorisation.AppUser.Update", "Failed to update AppUser"));
        }

        return user;
    }
}
