namespace Constellation.Application.Domains.Messaging.History.Queries.GetMessageDetails;

using Abstractions.Messaging;
using Core.Primitives;

public sealed record GetMessageDetailsQuery(
    IStronglyTypedId MessageId)
    : IQuery<MessageDetailResponse>;
