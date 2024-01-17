namespace Constellation.Application.ThirdPartyConsent.Events.ConsentTransactionReceivedDomainEvent;

using Abstractions.Messaging;
using Core.Models.ThirdPartyConsent.Events;
using System.Threading;
using System.Threading.Tasks;

internal class SendCopyToSubmitter
    : IDomainEventHandler<ConsentTransactionReceivedDomainEvent>
{
    public SendCopyToSubmitter()
    {
        
    }

    public async Task Handle(ConsentTransactionReceivedDomainEvent notification, CancellationToken cancellationToken)
    {

    }
}
