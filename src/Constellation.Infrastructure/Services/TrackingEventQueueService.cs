namespace Constellation.Infrastructure.Services;

using Application.Interfaces.Services;
using Core.Models.Messaging.Tracking;
using Core.Models.Messaging.Tracking.Identifiers;
using Microsoft.Extensions.DependencyInjection;
using Persistence.ConstellationContext;
using System.Text.Json;

internal sealed class TrackingEventQueueService
: ITrackingEventQueueService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false
    };

    public TrackingEventQueueService(
        IServiceScopeFactory scopeFactory, 
        ILogger logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger
            .ForContext<ITrackingEventQueueService>();
    }

    public async Task EnqueueAsync(TrackingEvent evt)
    {
        try
        {
            TrackingQueueEntry entry = new()
            {
                EventType = evt.GetType().Name, Payload = JsonSerializer.Serialize(evt, evt.GetType(), _jsonOptions)
            };

            using IServiceScope scope = _scopeFactory.CreateScope();
            AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.ChangeTracker.AutoDetectChangesEnabled = false;
            context.Set<TrackingQueueEntry>().Add(entry);
            await context.SaveChangesAsync();

            _logger
                .ForContext("EventType", evt.GetType().Name)
                .ForContext("EventId", evt.Id.ToString())
                .ForContext(nameof(TrackingQueueEntryId), entry.Id.ToString())
                .Information("Tracking event enqueued");
        }
        catch (Exception ex)
        {
            _logger
                .Error(ex, "Failed to enqueue tracking event");

            throw;
        }
    }
}