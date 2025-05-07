namespace Constellation.Application.Domains.Edval.Events.EdvalTeachersUpdated;

using Abstractions.Messaging;
using Core.Models.Edval.Events;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CalculateDifferences : IIntegrationEventHandler<EdvalTeachersUpdatedIntegrationEvent>
{
    public CalculateDifferences()
    {
        
    }
    public async Task Handle(EdvalTeachersUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        
    }
}