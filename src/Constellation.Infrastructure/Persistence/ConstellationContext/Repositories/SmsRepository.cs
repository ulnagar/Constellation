namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Core.Models.Messaging.Sms;
using Core.Models.Messaging.Sms.Identifiers;
using Core.Models.Messaging.Sms.Repositories;
using Core.ValueObjects;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<SmsMessage>> GetByNumber(
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SmsMessage>()
            .Where(message => 
                message.Sender.Number == phoneNumber.ToString(PhoneNumber.Format.None) ||
                message.Recipient.Number == phoneNumber.ToString(PhoneNumber.Format.None))
            .ToListAsync(cancellationToken);

    public async Task<List<SmsMessage>> GetRecent(
        int count,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<SmsMessage>()
            .OrderByDescending(message => message.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);

    public void Insert(SmsMessage message) => _context.Set<SmsMessage>().Add(message);
}
