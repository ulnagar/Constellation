namespace Constellation.Application.Domains.Affirmations.Queries;

using Abstractions.Messaging;

public sealed record GetAffirmationQuery(
    string UserId)
    : IQuery<string>;