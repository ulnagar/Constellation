namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Jobs;
using Application.Interfaces.Repositories;
using Constellation.Core.Models.Messaging.Enums;
using Constellation.Core.Models.Messaging.Tracking;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Enums;
using Core.Models.Messaging.Email.Identifiers;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Tracking.Identifiers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text.Json;

internal sealed class ProcessTrackingEventsJob : IProcessTrackingEventsJob
{
    private readonly AppDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
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
        AppDbContext context,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<IProcessTrackingEventsJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        _logger.Information("Tracking event processor started");

        await ProcessBatch(cancellationToken);

        _logger.Information("Tracking event processor stopped");
    }

    private async Task ProcessBatch(CancellationToken cancellationToken)
    {
        List<TrackingQueueEntry> entries = [];
        await foreach (TrackingQueueEntry entry in _getPendingEntries(_context, DateTime.UtcNow))
            entries.Add(entry);

        if (entries.Count == 0) return;

        _logger.
            Debug("Processing batch of {Count} tracking events", entries.Count);

        foreach (TrackingQueueEntry entry in entries)
            await ProcessEntry(entry, cancellationToken);

        await _unitOfWork.CompleteAsync(cancellationToken);
    }

    private async Task ProcessEntry(TrackingQueueEntry entry, CancellationToken cancellationToken)
    {
        try
        {
            TrackingEvent? evt = Deserialize(entry);

            if (evt is null)
            {
                _logger
                    .ForContext(nameof(TrackingQueueEntryId), entry.Id.ToString())
                    .ForContext(nameof(TrackingQueueEntry.EventType), entry.EventType)
                    .ForContext("Attempt", entry.Attempts + 1)
                    .Error("Failed to deserialize payload — dropping entry. Payload: {Payload}", entry.Payload);

                await _context.Set<TrackingQueueEntry>()
                    .Where(e => e.Id == entry.Id)
                    .ExecuteDeleteAsync(cancellationToken);

                return;
            }

            bool parentExists = await ParentExists(evt, _context, cancellationToken);

            if (!parentExists)
            {
                int nextAttempt = entry.Attempts + 1;

                if (nextAttempt >= _maxAttempts)
                {
                    _logger
                        .ForContext(nameof(TrackingQueueEntryId), entry.Id.ToString())
                        .ForContext(nameof(TrackingQueueEntry.EventType), entry.EventType)
                        .ForContext("Attempt", entry.Attempts + 1)
                        .Warning("Dropping tracking event after {MaxAttempts} attempts — parent record never appeared. EnqueuedAt: {EnqueuedAt}", _maxAttempts, entry.EnqueuedAt);

                    await _context.Set<TrackingQueueEntry>()
                        .Where(e => e.Id == entry.Id)
                        .ExecuteDeleteAsync(cancellationToken);
                    return;
                }

                double backoffMs = 500 * nextAttempt;

                _logger
                    .ForContext(nameof(TrackingQueueEntryId), entry.Id.ToString())
                    .ForContext(nameof(TrackingQueueEntry.EventType), entry.EventType)
                    .ForContext("Attempt", entry.Attempts + 1)
                    .Debug("Parent record not found, requeueing with {BackoffMs}ms backoff", backoffMs);

                await _context.Set<TrackingQueueEntry>()
                    .Where(e => e.Id == entry.Id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(e => e.Attempts, nextAttempt)
                        .SetProperty(e => e.RetryAfter, DateTime.UtcNow.AddMilliseconds(backoffMs)), cancellationToken);
                return;
            }

            await (evt switch
            {
                EmailOpenEvent e => HandleEmailOpen(e, _context, cancellationToken),
                EmailClickEvent e => HandleEmailClick(e, _context, cancellationToken),
                SmsDeliveryReceiptEvent e => HandleSmsDelivery(e, _context, cancellationToken),
                _ => throw new InvalidOperationException($"Unknown event type: {entry.EventType}")
            });

            // Delete the queue entry on success
            await _context.Set<TrackingQueueEntry>()
                .Where(e => e.Id == entry.Id)
                .ExecuteDeleteAsync(cancellationToken);

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

            await _context.Set<TrackingQueueEntry>()
                .Where(e => e.Id == entry.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.Attempts, entry.Attempts + 1)
                    .SetProperty(e => e.RetryAfter, DateTime.UtcNow.AddSeconds(5))
                    .SetProperty(e => e.LastError, ex.Message), cancellationToken);
        }
    }
    private static TrackingEvent? Deserialize(TrackingQueueEntry entry) =>
        entry.EventType switch
        {
            nameof(EmailOpenEvent) => JsonSerializer.Deserialize<EmailOpenEvent>(entry.Payload),
            nameof(EmailClickEvent) => JsonSerializer.Deserialize<EmailClickEvent>(entry.Payload),
            nameof(SmsDeliveryReceiptEvent) => JsonSerializer.Deserialize<SmsDeliveryReceiptEvent>(entry.Payload),
            _ => null
        };

    private static async Task<bool> ParentExists(TrackingEvent evt, AppDbContext context, CancellationToken ct) =>
        evt switch
        {
            EmailOpenEvent e => await context.Set<EmailMessage>().AnyAsync(m => m.Id == e.EmailId, ct),
            EmailClickEvent e => await context.Set<EmailMessage>().AnyAsync(m => m.Id == e.EmailId, ct),
            SmsDeliveryReceiptEvent e => await context.Set<SmsMessage>().AnyAsync(m => m.OutgoingId == e.OutgoingId, ct),
            _ => true
        };

    private static async Task HandleEmailOpen(EmailOpenEvent evt, AppDbContext context, CancellationToken ct)
    {
        context.Set<EmailTrackingEvent>().Add(new EmailTrackingEvent
        {
            Id = new(),
            EmailId = evt.EmailId,
            EventType = EmailEventType.Opened,
            OccurredAt = evt.OccurredAt,
            IpAddress = evt.IpAddress,
            UserAgent = evt.UserAgent
        });

        await context.Set<EmailMessage>()
            .Where(m => m.Id == evt.EmailId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(m => m.OpenCount, m => m.OpenCount + 1)
                .SetProperty(m => m.LastOpenedAt, evt.OccurredAt)
                .SetProperty(m => m.FirstOpenedAt, m => m.FirstOpenedAt ?? evt.OccurredAt), ct);
    }

    private async Task HandleEmailClick(EmailClickEvent evt, AppDbContext context, CancellationToken ct)
    {
        var now = evt.OccurredAt;

        var linkUpdated = await context.Set<EmailLink>()
            .Where(link => link.EmailId == evt.EmailId
                            && link.DestinationUrl == evt.DestinationUrl)
            .ExecuteUpdateAsync(s => s
                    .SetProperty(link => link.ClickCount, link => link.ClickCount + 1)
                    .SetProperty(link => link.LastClickedAt, now)
                    .SetProperty(link => link.FirstClickedAt, link => link.FirstClickedAt ?? now),
                ct);

        if (linkUpdated == 0)
        {
            _logger
                .ForContext(nameof(EmailId), evt.EmailId.ToString())
                .ForContext(nameof(EmailClickEvent.DestinationUrl), evt.DestinationUrl)
                .Warning("Click received for unregistered link");
        }

        context.Set<EmailTrackingEvent>().Add(new EmailTrackingEvent()
        {
            EmailId = evt.EmailId,
            EventType = EmailEventType.Clicked,
            OccurredAt = evt.OccurredAt,
            IpAddress = evt.IpAddress,
            UserAgent = evt.UserAgent,
            LinkUrl = evt.DestinationUrl
        });

        await context
            .Set<EmailMessage>()
            .Where(message => message.Id == evt.EmailId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(message => message.ClickCount, message => message.ClickCount + 1)
                .SetProperty(message => message.LastClickedAt, evt.OccurredAt)
                .SetProperty(message => message.FirstClickedAt, message => message.FirstClickedAt ?? evt.OccurredAt),
                ct);
    }

    private static async Task HandleSmsDelivery(SmsDeliveryReceiptEvent evt, AppDbContext context, CancellationToken ct)
    {
        IQueryable<SmsMessage> query = context.Set<SmsMessage>()
            .Where(m => m.OutgoingId == evt.OutgoingId);

        if (evt.Status is not null)
        {
            await query.ExecuteUpdateAsync(s => s
                .SetProperty(m => m.Status, evt.Status switch
                {
                    "Delivered" => MessageStatus.Delivered,
                    "Failed" => MessageStatus.Error,
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
