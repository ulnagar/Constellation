namespace Constellation.Infrastructure.Services;

using Application.Interfaces.Services;
using Core.Models.Messaging.EmergencyConsole.Identifiers;
using Core.Models.Messaging.EmergencyConsole.Services;
using Hangfire;
using System.Threading.Tasks;

internal sealed class HangfireJobService : IHangfireJobService
{
    public async Task EnqueueEmergencyMessageJob(EventId eventId, CancellationToken cancellationToken = default)
    {
        BackgroundJob.Enqueue<IEmergencyService>(service => service.SendEmergencyAlerts(eventId, cancellationToken));
    }
}
