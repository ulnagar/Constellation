using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.DomainEvents;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.ClassCovers.Events;

internal sealed class CoverCreatedDomainEvent_CreateAdobeConnectAccessHandler
    : IDomainEventHandler<CoverCreatedDomainEvent>
{
    public async Task Handle(CoverCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
