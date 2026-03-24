namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.Messaging.Enums;
using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Identifiers;
using Core.Models.Messaging.Sms.Repositories;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using System;

internal sealed class SmsRepository : ISmsRepository
{
    private readonly AppDbContext _context;

    public SmsRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<SmsMessage?> GetById(
        SmsId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SmsMessage>()
            .FirstOrDefaultAsync(message => message.Id == id, cancellationToken);

    public async Task<SmsMessage?> GetByOutgoingId(
        string outgoingId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SmsMessage>()
            .Where(message => message.OutgoingId == outgoingId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<SmsMessage?> GetMostRecentOutboundToNumber(
        string phoneNumber, 
        CancellationToken cancellationToken = default)
    {
        var windowStart = DateTimeOffset.UtcNow.AddHours(-24);
        var originalMessage = await _context
            .Set<SmsMessage>()
            .Where(message => message.To == phoneNumber
                        && message.CreatedAt >= windowStart
                        && message.Direction == MessageDirection.Outbound)
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return originalMessage;
    }

    public async Task<List<SmsMessage>> GetByNumber(
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SmsMessage>()
            .Where(message => 
                message.From == phoneNumber.ToString(PhoneNumber.Format.International) ||
                message.From == phoneNumber.ToString(PhoneNumber.Format.None) ||
                message.To == phoneNumber.ToString(PhoneNumber.Format.International) ||
                message.To == phoneNumber.ToString(PhoneNumber.Format.None))
            .ToListAsync(cancellationToken);

    public void Insert(SmsMessage message) => _context.Set<SmsMessage>().Add(message);
}
