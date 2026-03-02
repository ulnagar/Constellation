namespace Constellation.Application.Domains.Messaging.Sms.Repositories;

using Identifiers;
using Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

public interface ISmsRepository
{
    Task<SmsMessage?> GetById(SmsId id, CancellationToken cancellationToken = default);
    Task<SmsMessage?> GetByOutgoingId(string outgoingId, CancellationToken cancellationToken = default);
    Task<SmsMessage?> GetMostRecentOutboundToNumber(string phoneNumber, CancellationToken cancellationToken = default);

    void Insert(SmsMessage message);
}
