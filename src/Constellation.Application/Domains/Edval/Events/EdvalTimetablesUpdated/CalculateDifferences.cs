namespace Constellation.Application.Domains.Edval.Events.EdvalTimetablesUpdated;

using Abstractions.Messaging;
using Core.Models.Edval.Events;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CalculateDifferences : IIntegrationEventHandler<EdvalTimetablesUpdatedIntegrationEvent>
{
    public CalculateDifferences()
    {
        
    }

    public async Task Handle(EdvalTimetablesUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        
    }
}