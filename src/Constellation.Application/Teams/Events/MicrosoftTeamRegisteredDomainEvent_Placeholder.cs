namespace Constellation.Core.DomainEvents;

using Constellation.Application.Abstractions.Messaging;
using System.Threading;
using System.Threading.Tasks;

internal sealed class MicrosoftTeamRegisteredDomainEvent_Placeholder
    : IDomainEventHandler<MicrosoftTeamRegisteredDomainEvent>
{
    private readonly Serilog.ILogger _logger;

    public MicrosoftTeamRegisteredDomainEvent_Placeholder(Serilog.ILogger logger)
    {
        _logger = logger.ForContext<MicrosoftTeamRegisteredDomainEvent>();
    }

    public async Task Handle(MicrosoftTeamRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Microsoft Team has been registered: {@details}", notification);
    }
}