namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberResignedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Events;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
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
        StaffMember staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

        // Remove user access
        UserTemplateDto userDetails = new()
        {
            FirstName = staffMember.Name.FirstName,
            LastName = staffMember.Name.LastName,
            Email = staffMember.EmailAddress.Email,
            Username = staffMember.EmailAddress.Email,
            IsStaffMember = false
        };

        if (_userManager.Users.Any(u => u.UserName == userDetails.Username))
        {
            AppUser user = await _userManager.FindByEmailAsync(userDetails.Email);

            user!.IsStaffMember = false;
            user.StaffId = StaffId.Empty;

            await _userManager.RemoveFromRoleAsync(user, AuthRoles.StaffMember);

            await _userManager.UpdateAsync(user);
        }
    }
}
