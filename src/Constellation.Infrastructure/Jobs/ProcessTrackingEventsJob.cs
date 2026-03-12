namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Jobs;
using Constellation.Core.Models.Messaging.Sms.Enums;
using Constellation.Core.Models.Messaging.Tracking;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Enums;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Tracking.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Text.Json;

internal sealed class ProcessTrackingEventsJob : IProcessTrackingEventsJob
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private const int _maxAttempts = 5;
    private const int _batchSize = 50;

    private static readonly Func<AppDbContext, DateTime, IAsyncEnumerable<TrackingQueueEntry>> _getPendingEntries =
        EF.CompileAsyncQuery((AppDbContext db, DateTime now) =>
            db.Set<TrackingQueueEntry>()
                .Where(e => e.RetryAfter == null || e.RetryAfter <= now)
                .OrderBy(e => e.EnqueuedAt)
                .Take(_batchSize)
                .AsNoTracking());

    public ProcessTrackingEventsJob(
        IServiceScopeFactory scopeFactory,
        ILogger logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger
            .ForContext<IProcessTrackingEventsJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.Information("Tracking event processor started");

        while (!cancellationToken.IsCancellationRequested)
        {
            await ProcessBatch(cancellationToken);
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
        }
    }

    private async Task ProcessBatch(CancellationToken ct)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        List<TrackingQueueEntry> entries = [];
        await foreach (TrackingQueueEntry entry in _getPendingEntries(db, DateTime.UtcNow))
            entries.Add(entry);

        if (entries.Count == 0) return;

        _logger.
            Debug("Processing batch of {Count} tracking events", entries.Count);

        foreach (TrackingQueueEntry entry in entries)
            await ProcessEntry(entry, ct);
    }

    private async Task ProcessEntry(TrackingQueueEntry entry, CancellationToken ct)
    {
        using (LogContext.PushProperty("QueueEntryId", entry.Id))
        using (LogContext.PushProperty("EventType", entry.EventType))
        using (LogContext.PushProperty("Attempt", entry.Attempts + 1))
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                var evt = Deserialize(entry);

                if (evt is null)
                {
                    _logger
                        .ForContext(nameof(TrackingQueueEntryId), entry.Id.ToString())
                        .ForContext(nameof(TrackingQueueEntry.EventType), entry.EventType)
                        .ForContext("Attempt", entry.Attempts + 1)
                        .Error("Failed to deserialize payload — dropping entry. Payload: {Payload}", entry.Payload);

                    await db.Set<TrackingQueueEntry>()
                        .Where(e => e.Id == entry.Id)
                        .ExecuteDeleteAsync(ct);

                    return;
                }

                var parentExists = await ParentExists(evt, db, ct);

                if (!parentExists)
                {
                    var nextAttempt = entry.Attempts + 1;

                    if (nextAttempt >= _maxAttempts)
                    {
                        _logger
                            .ForContext(nameof(TrackingQueueEntryId), entry.Id.ToString())
                            .ForContext(nameof(TrackingQueueEntry.EventType), entry.EventType)
                            .ForContext("Attempt", entry.Attempts + 1)
                            .Warning("Dropping tracking event after {MaxAttempts} attempts — parent record never appeared. EnqueuedAt: {EnqueuedAt}", _maxAttempts, entry.EnqueuedAt);

                        await db.Set<TrackingQueueEntry>()
                            .Where(e => e.Id == entry.Id)
                            .ExecuteDeleteAsync(ct);
                        return;
                    }

                    double backoffMs = 500 * nextAttempt;

                    _logger
                        .ForContext(nameof(TrackingQueueEntryId), entry.Id.ToString())
                        .ForContext(nameof(TrackingQueueEntry.EventType), entry.EventType)
                        .ForContext("Attempt", entry.Attempts + 1)
                        .Debug("Parent record not found, requeueing with {BackoffMs}ms backoff", backoffMs);

                    await db.Set<TrackingQueueEntry>()
                        .Where(e => e.Id == entry.Id)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(e => e.Attempts, nextAttempt)
                            .SetProperty(e => e.RetryAfter, DateTime.UtcNow.AddMilliseconds(backoffMs)), ct);
                    return;
                }

                await (evt switch
                {
                    EmailOpenEvent e => HandleEmailOpen(e, db, ct),
                    SmsDeliveryReceiptEvent e => HandleSmsDelivery(e, db, ct),
                    _ => throw new InvalidOperationException($"Unknown event type: {entry.EventType}")
                });

                // Delete the queue entry on success
                await db.Set<TrackingQueueEntry>()
                    .Where(e => e.Id == entry.Id)
                    .ExecuteDeleteAsync(ct);

                _logger
                    .ForContext(nameof(TrackingQueueEntryId), entry.Id.ToString())
                    .ForContext(nameof(TrackingQueueEntry.EventType), entry.EventType)
                    .ForContext("Attempt", entry.Attempts + 1)
                    .Information("Tracking event processed and removed from queue");
            }
            catch (Exception ex)
            {
                _logger
                    .ForContext(nameof(TrackingQueueEntryId), entry.Id.ToString())
                    .ForContext(nameof(TrackingQueueEntry.EventType), entry.EventType)
                    .ForContext("Attempt", entry.Attempts + 1)
                    .Error(ex, "Unhandled exception — requeueing entry. EnqueuedAt: {EnqueuedAt}", entry.EnqueuedAt);

                await db.Set<TrackingQueueEntry>()
                    .Where(e => e.Id == entry.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.Attempts, entry.Attempts + 1)
                        .SetProperty(e => e.RetryAfter, DateTime.UtcNow.AddSeconds(5))
                        .SetProperty(e => e.LastError, ex.Message), ct);
            }
        }
    }
    private static TrackingEvent? Deserialize(TrackingQueueEntry entry) =>
        entry.EventType switch
        {
            nameof(EmailOpenEvent) => JsonSerializer.Deserialize<EmailOpenEvent>(entry.Payload),
            nameof(SmsDeliveryReceiptEvent) => JsonSerializer.Deserialize<SmsDeliveryReceiptEvent>(entry.Payload),
            _ => null
        };

    private static async Task<bool> ParentExists(TrackingEvent evt, AppDbContext db, CancellationToken ct) =>
        evt switch
        {
            EmailOpenEvent e => await db.Set<EmailMessage>().AnyAsync(m => m.Id == e.EmailId, ct),
            SmsDeliveryReceiptEvent e => await db.Set<SmsMessage>().AnyAsync(m => m.OutgoingId == e.OutgoingId, ct),
            _ => true
        };

    private static async Task HandleEmailOpen(EmailOpenEvent evt, AppDbContext db, CancellationToken ct)
    {
        db.Set<EmailTrackingEvent>().Add(new EmailTrackingEvent
        {
            EmailId = evt.EmailId,
            EventType = EmailEventType.Opened,
            OccurredAt = evt.OccurredAt,
            IpAddress = evt.IpAddress,
            UserAgent = evt.UserAgent
        });

        await db.Set<EmailMessage>()
            .Where(m => m.Id == evt.EmailId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.OpenCount, m => m.OpenCount + 1)
                .SetProperty(m => m.LastOpenedAt, evt.OccurredAt)
                .SetProperty(m => m.FirstOpenedAt, m => m.FirstOpenedAt ?? evt.OccurredAt), ct);
    }

    private static async Task HandleSmsDelivery(SmsDeliveryReceiptEvent evt, AppDbContext db, CancellationToken ct)
    {
        IQueryable<SmsMessage> query = db.Set<SmsMessage>()
            .Where(m => m.OutgoingId == evt.OutgoingId);

        if (evt.Status is not null)
        {
            await query.ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, evt.Status switch
                {
                    "Delivered" => SmsStatus.Delivered,
                    "Failed" => SmsStatus.Failed,
                    _ => throw new ArgumentOutOfRangeException()
                })
                .SetProperty(m => m.SmsGlobalDate, evt.OccurredAt), ct);
        }
        else
        {
            await query.ExecuteUpdateAsync(s => s
                .SetProperty(m => m.SmsGlobalDate, evt.OccurredAt), ct);
        }
    }
}
