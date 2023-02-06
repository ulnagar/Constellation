using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.DomainEvents;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.ClassCovers.Events;

internal sealed class CoverEndDateChangedDomainEvent_SendCoverUpdatedEmailHandler
    : IDomainEventHandler<CoverEndDateChangedDomainEvent>
{

    public async Task Handle(CoverEndDateChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}
