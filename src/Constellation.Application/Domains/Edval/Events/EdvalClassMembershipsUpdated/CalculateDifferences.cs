namespace Constellation.Application.Domains.Edval.Events.EdvalClassMembershipsUpdated;

using Application.Abstractions.Messaging;
using Constellation.Core.Models.Edval.Events;
using System.Threading;
using System.Threading.Tasks;

internal sealed class CalculateDifferences : IIntegrationEventHandler<EdvalClassMembershipsUpdatedIntegrationEvent>
{
    public CalculateDifferences()
    {
        
    }
    public async Task Handle(EdvalClassMembershipsUpdatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        
    }
}