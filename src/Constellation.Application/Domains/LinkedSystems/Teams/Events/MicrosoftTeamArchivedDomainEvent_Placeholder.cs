namespace Constellation.Application.Domains.LinkedSystems.Teams.Events;

using Abstractions.Messaging;
using Core.DomainEvents;
using System.Threading;
using System.Threading.Tasks;

internal sealed class MicrosoftTeamArchivedDomainEvent_Placeholder
    : IDomainEventHandler<MicrosoftTeamArchivedDomainEvent>
{
    private readonly Serilog.ILogger _logger;

    public MicrosoftTeamArchivedDomainEvent_Placeholder(Serilog.ILogger logger)
    {
        _logger = logger.ForContext<MicrosoftTeamArchivedDomainEvent>();
    }

    public async Task Handle(MicrosoftTeamArchivedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.Information("Microsoft Team has been marked archived: {@details}", notification);
    }
}