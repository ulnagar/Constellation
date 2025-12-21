namespace Constellation.Infrastructure.Services;

using Application.Interfaces.Services;
using Core.Models.EmergencyConsole.Identifiers;
using Core.Models.EmergencyConsole.Services;
using Hangfire;
using System.Threading.Tasks;

internal sealed class HangfireJobService : IHangfireJobService
{
    public async Task EnqueueEmergencyMessageJob(EventId eventId, CancellationToken cancellationToken = default)
    {
        BackgroundJob.Enqueue<IEmergencyService>(service => service.SendEmergencyAlerts(eventId, cancellationToken));
    }
}
