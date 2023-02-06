using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.DomainEvents;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.ClassCovers.Events;

internal sealed class CoverStartAndEndDatesChangedDomainEvent_UpdateAdobeConnectAccessHandler
    : IDomainEventHandler<CoverStartAndEndDatesChangedDomainEvent>
{

    public async Task Handle(CoverStartAndEndDatesChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
