namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberEmailAddressChangedDomainEvent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models.StaffMembers;
using Constellation.Core.Models.StaffMembers.Errors;
using Constellation.Core.Models.StaffMembers.Events;
using Constellation.Core.Models.StaffMembers.Repositories;
using Constellation.Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CreateOrUpdateUserAccount
: IDomainEventHandler<StaffMemberEmailAddressChangedDomainEvent>
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

    public async Task Handle(StaffMemberEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        StaffMember staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(StaffMemberEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(notification.StaffId), true)
                .Warning("Failed to update Staff Member AppUser email address");
            return;
        }

        AppUser user = await _userManager.FindByEmailAsync(notification.OldEmailAddress);

        if (user is not null)
        {
            user.IsStaffMember = true;
            user.StaffId = staffMember.Id;
            user.Email = notification.NewEmailAddress;
            user.UserName = notification.NewEmailAddress;

            IdentityResult update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(StaffMemberEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to update Staff Member AppUser email address");
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
                .ForContext(nameof(StaffMemberEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(IdentityResult.Errors), create.Errors, true)
                .Warning("Failed to update Staff Member AppUser email address");
        }
    }
}
