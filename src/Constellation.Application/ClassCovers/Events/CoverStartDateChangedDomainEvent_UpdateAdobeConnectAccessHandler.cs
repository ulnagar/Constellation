using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.DomainEvents;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.ClassCovers.Events;

internal sealed class CoverStartDateChangedDomainEvent_UpdateAdobeConnectAccessHandler
    : IDomainEventHandler<CoverStartDateChangedDomainEvent>
{

    public async Task Handle(CoverStartDateChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
