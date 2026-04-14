namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberReinstatedDomainEvent;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Enums;
using Core.Models.Auth;
using Core.Models.Auth.Enums;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Events;
using Core.Models.StaffMembers.Repositories;
using Core.Shared;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Data;
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
        StaffMember? staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(StaffMemberReinstatedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(notification.StaffId), true)
                .Warning("Failed to create new Staff Member AppUser");
            return;
        }

        AppUser? user = await _userManager.FindByEmailAsync(staffMember.EmailAddress.Email);

        if (user is null)
        {
            user = new()
            {
                UserName = staffMember.EmailAddress.Email,
                Email = staffMember.EmailAddress.Email,
                Name = staffMember.Name
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

        List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Staff).ToList();

        if (links.All(link => link.LinkId != staffMember.Id.Value))
        {
            user.AddStaffLink(staffMember.Id);

            IdentityResult update = await _userManager.UpdateAsync(user);

            if (!update.Succeeded)
            {
                _logger
                    .ForContext(nameof(StaffMemberReinstatedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                    .Warning("Failed to create new Staff Member AppUser");
            }
        }
        
        await _userManager.AddToRoleAsync(user, AppRole.Staff);
    }
}
