namespace Constellation.Application.Domains.Messaging.Sms.Repositories;

using Identifiers;
using Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

public interface ISmsRepository
{
    Task<SmsMessage?> GetById(SmsId id);
    Task<SmsMessage?> GetByOutgoingId(string outgoingId);

    void Insert(SmsMessage message);
}
