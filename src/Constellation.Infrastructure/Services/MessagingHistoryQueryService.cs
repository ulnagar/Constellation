namespace Constellation.Infrastructure.Services;

using Application.Domains.Messaging.History.Models;
using Application.Interfaces.Services;
using Application.Models;
using Core.Models.Messaging.Email;
using Core.Models.Messaging.Email.Enums;
using Core.Models.Messaging.Email.Identifiers;
using Core.Models.Messaging.Enums;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Identifiers;
using Microsoft.EntityFrameworkCore;
using Persistence.ConstellationContext;
using Persistence.ConstellationContext.Views;
using System;
using System.Linq.Expressions;

internal class MessagingHistoryQueryService : IMessagingHistoryQueryService
{
    private readonly ConstellationDbContext _context;

    public MessagingHistoryQueryService(
        ConstellationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<CommunicationRecordResponse>> GetRecentHistory(
        string? searchQuery, 
        MessagingHistoryDateRange dateRange,
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<MessagingHistoryIndexRow>().AsQueryable();

        DateTimeOffset? cutoff = dateRange switch
        {
            MessagingHistoryDateRange.Last30Days => DateTimeOffset.UtcNow.AddDays(-30),
            MessagingHistoryDateRange.CurrentCalendarYear => new DateTimeOffset(
                new DateOnly(DateTime.UtcNow.Year, 1, 1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
            MessagingHistoryDateRange.AllTime => null,
            _ => throw new ArgumentOutOfRangeException(nameof(dateRange))
        };

        if (cutoff is not null)
            query = query.Where(r => r.CreatedAt >= cutoff);

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            if (searchQuery == "sms")
            {
                query = query.Where(r => r.MessageType == MessageType.SMS.Value);
            }
            else if (searchQuery == "email")
            {
                query = query.Where(r => r.MessageType == MessageType.Email.Value);
            }
            else
            {
                string[] terms = searchQuery.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

                query = query.Where(r => terms.All(t =>
                    r.Subject.Contains(t) ||
                    r.FromName.Contains(t) ||
                    (r.FromAddress != null && r.FromAddress.Contains(t)) ||
                    (r.RecipientSearchText != null && r.RecipientSearchText.Contains(t)) ||
                    (r.BodyText != null && r.BodyText.Contains(t))));
            }
        }

        int totalCount = await query.CountAsync(cancellationToken);

        var page = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var emailIds = page
            .Where(x => x.MessageTypeValue == MessageType.Email)
            .Select(x => EmailId.FromValue(x.Id))
            .ToList();

        var smsIds = page
            .Where(x => x.MessageTypeValue == MessageType.SMS)
            .Select(x => SmsId.FromValue(x.Id))
            .ToList();

        var emails = await _context.Set<EmailMessage>()
            .Where(e => emailIds.Contains(e.Id))
            .ToListAsync(cancellationToken);

        var sms = await _context.Set<SmsMessage>()
            .Where(s => smsIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        var responseById = emails.Select(MapEmailToResponse)
            .Concat(sms.Select(MapSmsToResponse))
            .ToDictionary(r => r.Id.ToString());

        var records = page
            .Select(entry => responseById[entry.Id.ToString()])
            .ToList();

        return new PaginatedList<CommunicationRecordResponse>(records, totalCount, pageNumber, pageSize);
    }

    private static Expression<Func<EmailMessage, bool>> BuildEmailSearchPredicate(string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            return _ => true;

        string[] terms = searchQuery.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return email => terms.All(t =>
            email.Subject.Contains(t) ||
            email.From.Name.Contains(t) ||
            email.From.Destination.Contains(t) ||
            email.Recipients.Any(r => r.Name.Contains(t) || r.Email.Contains(t)) ||
            email.BodyText.Contains(t));
    }

    private static Expression<Func<SmsMessage, bool>> BuildSmsSearchPredicate(string? searchQuery)
    {
        if (string.IsNullOrWhiteSpace(searchQuery))
            return _ => true;

        string[] terms = searchQuery.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return sms => terms.All(t =>
            sms.Message.Contains(t) ||
            sms.Sender.Name.Contains(t) ||
            sms.Recipient.Name.Contains(t) || 
            sms.Recipient.Value.Contains(t) ||
            sms.Message.Contains(t));
    }

    private static CommunicationRecordResponse MapEmailToResponse(EmailMessage email)
    {
        List<CommunicationRecordResponse.Recipient> recipients = [];

        foreach (EmailMessageRecipient recipient in email.Recipients)
        {
            recipients.Add(new(
                recipient.RecipientType,
                recipient.Name,
                recipient.Email));
        }

        return new(
            email.Id,
            MessageType.Email,
            MessageDirection.Outbound,
            new(email.From.Name, email.From.Destination),
            recipients,
            email.Subject,
            email.Status,
            email.CreatedAt);
    }

    private static CommunicationRecordResponse MapSmsToResponse(SmsMessage sms)
    {
        List<CommunicationRecordResponse.Recipient> recipients =
        [
            new(
                EmailRecipientType.To,
                sms.Recipient.Name,
                sms.Recipient.Number)
        ];

        return new(
            sms.Id,
            MessageType.SMS,
            sms.Direction,
            new(sms.Sender.Name, sms.Sender.Number),
            recipients,
            sms.Message,
            sms.Status,
            sms.CreatedAt);
    }
}
