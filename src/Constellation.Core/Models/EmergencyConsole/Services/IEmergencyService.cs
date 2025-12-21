namespace Constellation.Core.Models.EmergencyConsole.Services;

using Identifiers;

public interface IEmergencyService
{
    Task SendEmergencyAlerts(EventId eventId, CancellationToken cancellationToken = default);
}