namespace Constellation.Application.WorkFlows.Events.CaseCreatedDomainEvent;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Events;
using System.Threading;
using System.Threading.Tasks;

internal sealed class AddUploadTrainingCertificationActionForTrainingCase
    : IDomainEventHandler<CaseCreatedDomainEvent>
{
    public AddUploadTrainingCertificationActionForTrainingCase()
    {
        
    }

    public async Task Handle(CaseCreatedDomainEvent notification, CancellationToken cancellationToken)
    {

    }
}