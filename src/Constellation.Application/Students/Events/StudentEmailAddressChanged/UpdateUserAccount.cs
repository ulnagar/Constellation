namespace Constellation.Application.Students.Events.StudentEmailAddressChanged;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models.Students.Events;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateUserAccount
: IDomainEventHandler<StudentEmailAddressChangedDomainEvent>
{
    private readonly UserManager<AppUser> _userManager;

    public UpdateUserAccount(
        UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(StudentEmailAddressChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        return string.Empty;
    }
}