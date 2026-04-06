namespace Constellation.Core.Models.Messaging.Sms.Repositories;

using Identifiers;
using ValueObjects;

public interface ISmsRepository
{
    Task<SmsMessage?> GetById(SmsId id, CancellationToken cancellationToken = default);
    Task<SmsMessage?> GetByOutgoingId(string outgoingId, CancellationToken cancellationToken = default);
    Task<List<SmsMessage>> GetByNumber(PhoneNumber phoneNumber, CancellationToken cancellationToken = default);
    Task<List<SmsMessage>> GetRecent(int count, CancellationToken cancellationToken = default);
    void Insert(SmsMessage message);
}
