namespace Constellation.Core.Models.Messaging.Sms.Repositories;

using Identifiers;

public interface ISmsRepository
{
    Task<SmsMessage?> GetById(SmsId id, CancellationToken cancellationToken = default);
    Task<SmsMessage?> GetByOutgoingId(string outgoingId, CancellationToken cancellationToken = default);
    Task<SmsMessage?> GetMostRecentOutboundToNumber(string phoneNumber, CancellationToken cancellationToken = default);

    void Insert(SmsMessage message);
}
