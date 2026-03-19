namespace Constellation.Application.Domains.Messaging.History.Queries;

using Abstractions.Messaging;
using Core.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

public sealed record GetCommunicationsHistoryForContactQuery(
    IStronglyTypedId Id)
    : IQuery<List<CommunicationRecordResponse>>;