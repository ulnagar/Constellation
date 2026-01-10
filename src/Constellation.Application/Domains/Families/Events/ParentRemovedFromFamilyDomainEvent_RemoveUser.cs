namespace Constellation.Application.Domains.Families.Events;

using Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Application.Models.Identity.Enums;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Families.Events;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ParentRemovedFromFamilyDomainEvent_RemoveUser
    : IDomainEventHandler<ParentRemovedFromFamilyDomainEvent>
{
    private readonly IFamilyRepository _familyRepository;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger _logger;

    public ParentRemovedFromFamilyDomainEvent_RemoveUser(
        IFamilyRepository familyRepository,
        UserManager<AppUser> userManager,
        ILogger logger)
    {
        _familyRepository = familyRepository;
        _userManager = userManager;
        _logger = logger
            .ForContext<ParentRemovedFromFamilyDomainEvent>();
    }

    public async Task Handle(ParentRemovedFromFamilyDomainEvent notification, CancellationToken cancellationToken)
    {
        AppUser? existingUser = await _userManager.FindByEmailAsync(notification.EmailAddress);

        if (existingUser is null)
            return;

        AppUserLink? existingLink = existingUser.Links.FirstOrDefault(link =>
            !link.IsDeleted && 
            link.Type == LinkType.Parent && 
            link.LinkId == notification.ParentId.Value);

        if (existingLink is not null)
        {
            existingLink.Delete();

            await _userManager.UpdateAsync(existingUser);
        }

        int otherParents = await _familyRepository.CountOfParentsWithEmailAddress(notification.EmailAddress, cancellationToken);

        if (otherParents == 0)
        {
            IEnumerable<AppUserLink> links = existingUser.Links.Where(link => link.Type == LinkType.Parent && !link.IsDeleted);

            foreach (AppUserLink link in links)
                link.Delete();

            await _userManager.UpdateAsync(existingUser);
        }

        if (existingUser.Links.All(link => link.IsDeleted))
        {
            await _userManager.DeleteAsync(existingUser);
        }
    }
}
