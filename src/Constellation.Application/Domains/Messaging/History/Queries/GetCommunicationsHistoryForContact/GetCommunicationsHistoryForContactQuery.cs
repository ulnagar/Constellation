namespace Constellation.Application.Domains.Messaging.History.Queries.GetCommunicationsHistoryForContact;

using Abstractions.Messaging;
using Constellation.Application.Domains.Messaging.History.Models;
using Core.Primitives;

public sealed record GetCommunicationsHistoryForContactQuery(
    IStronglyTypedId Id)
    : IQuery<List<CommunicationRecordResponse>>;