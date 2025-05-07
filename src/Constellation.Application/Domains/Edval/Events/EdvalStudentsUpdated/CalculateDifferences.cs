namespace Constellation.Application.Domains.Edval.Events.EdvalStudentsUpdated;

using Abstractions.Messaging;
using Core.Models.Edval.Events;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CalculateDifferences : IIntegrationEventHandler<EdvalStudentsUpdatedIntegrationEvent>
{
    public CalculateDifferences()
    {
        
    }
    public async Task Handle(EdvalStudentsUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        
    }
}