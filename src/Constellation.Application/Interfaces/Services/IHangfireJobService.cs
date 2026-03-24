namespace Constellation.Application.Interfaces.Services;

using Core.Models.Messaging.EmergencyConsole.Identifiers;

public interface IHangfireJobService
{
    Task EnqueueEmergencyMessageJob(EventId eventId, CancellationToken cancellationToken = default);
}