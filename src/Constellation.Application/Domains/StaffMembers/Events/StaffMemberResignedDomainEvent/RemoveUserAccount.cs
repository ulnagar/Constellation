namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberResignedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Enums;
using Constellation.Core.Errors;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Events;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class RemoveUserAccount
: IDomainEventHandler<StaffMemberResignedDomainEvent>
{
    private readonly IStaffRepository _staffRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public RemoveUserAccount(
        IStaffRepository staffRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _staffRepository = staffRepository;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Handle(StaffMemberResignedDomainEvent notification, CancellationToken cancellationToken)
    {
        StaffMember? staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(StaffMemberResignedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(notification.StaffId), true)
                .Warning("Failed to delete old Staff Member AppUser");

            return;
        }
        
        AppUser? user = await _userManager.FindByEmailAsync(staffMember.EmailAddress.Email);

        if (user is null)
        {
            _logger
                .ForContext(nameof(StaffMemberResignedDomainEvent), notification, true)
                .ForContext(nameof(Error), DomainErrors.Auth.UserNotFound, true)
                .Warning("Failed to delete old Staff Member AppUser");

            return;
        }

        AppUserLink? link = user.Links
            .FirstOrDefault(link =>
                !link.IsDeleted &&
                link.Type == LinkType.Staff &&
                link.LinkId == staffMember.Id.Value);

        if (link is not null)
            link.Delete();

        await _userManager.UpdateAsync(user);

        if (user.Links.All(link => link.IsDeleted))
        {
            IdentityResult update = await _userManager.DeleteAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(StaffMemberResignedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to delete old Staff Member AppUser");
            }
        }
    }
}
