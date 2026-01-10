namespace Constellation.Application.Domains.StaffMembers.Events.StaffMemberEmailAddressChangedDomainEvent;

using Application.Models.Identity.Enums;
using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models.SchoolContacts.Events;
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
        StaffMember? staffMember = await _staffRepository.GetById(notification.StaffId, cancellationToken);

        if (staffMember is null)
        {
            _logger
                .ForContext(nameof(StaffMemberEmailAddressChangedDomainEvent), notification, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(notification.StaffId), true)
                .Warning("Failed to update Staff Member AppUser email address");
            return;
        }

        AppUser? user = await _userManager.FindByEmailAsync(notification.OldEmailAddress);

        if (user is not null)
        {
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
                        .ForContext(nameof(StaffMemberEmailAddressChangedDomainEvent), notification, true)
                        .ForContext(nameof(AppUser), user, true)
                        .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                        .Warning("Failed to update Staff Member AppUser email address");
                }
            }
        }

        user = await _userManager.FindByEmailAsync(notification.NewEmailAddress);

        if (user is not null)
        {
            List<AppUserLink> links = user.Links.Where(link => !link.IsDeleted && link.Type == LinkType.Staff).ToList();

            if (links.All(link => link.LinkId != staffMember.Id.Value))
            {
                user.AddStaffLink(staffMember.Id);

                IdentityResult update = await _userManager.UpdateAsync(user);

                if (!update.Succeeded)
                {
                    _logger
                        .ForContext(nameof(SchoolContactEmailAddressChangedDomainEvent), notification, true)
                        .ForContext(nameof(AppUser), user, true)
                        .ForContext(nameof(IdentityResult.Errors), update.Errors, true)
                        .Warning("Failed to update Staff Member AppUser email address");

                    return;
                }
            }
        }
        else
        {
            user = new()
            {
                UserName = staffMember.EmailAddress.Email,
                Email = staffMember.EmailAddress.Email,
                Name = staffMember.Name
            };

            user.AddStaffLink(staffMember.Id);

            IdentityResult create = await _userManager.CreateAsync(user);

            if (!create.Succeeded)
            {
                _logger
                    .ForContext(nameof(SchoolContactEmailAddressChangedDomainEvent), notification, true)
                    .ForContext(nameof(AppUser), user, true)
                    .ForContext(nameof(IdentityResult.Errors), create.Errors, true)
                    .Warning("Failed to update Staff Member AppUser email address");

                return;
            }
        }
    }
}
