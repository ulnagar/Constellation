namespace Constellation.Application.Interfaces.Services;

using Core.Models.EmergencyConsole.Identifiers;

public interface IHangfireJobService
{
    Task EnqueueEmergencyMessageJob(EventId eventId, CancellationToken cancellationToken = default);
}