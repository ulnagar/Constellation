namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberReinstatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Events;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateOrUpdateUserAccount
: IDomainEventHandler<StaffMemberReinstatedDomainEvent>
{
    private readonly IStaffRepository _staffRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public CreateOrUpdateUserAccount(
        IStaffRepository staffRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Handle(StaffMemberReinstatedDomainEvent notification, CancellationToken cancellationToken)
    {
        StaffMember staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(StaffMemberReinstatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(notification.StaffId), true)
                .Warning("Failed to create new Staff Member AppUser");
            return;
        }

        AppUser user = await _userManager.FindByEmailAsync(staffMember.EmailAddress.Email);

        if (user is not null)
        {
            user.IsStaffMember = true;
            user.StaffId = staffMember.Id;

            IdentityResult update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(StaffMemberReinstatedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to update Staff Member AppUser");
            }

            return;
        }

        user = new()
        {
            UserName = staffMember.EmailAddress.Email,
            Email = staffMember.EmailAddress.Email,
            FirstName = staffMember.Name.FirstName,
            LastName = staffMember.Name.LastName,
            IsStaffMember = true,
            StaffId = staffMember.Id
        };

        IdentityResult create = await _userManager.CreateAsync(user);

        if (create.Succeeded)
        {
            _logger
                .ForContext(nameof(StaffMemberReinstatedDomainEvent), notification, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(IdentityResult.Errors), create.Errors, true)
                .Warning("Failed to create new Staff Member AppUser");
        }
    }
}
