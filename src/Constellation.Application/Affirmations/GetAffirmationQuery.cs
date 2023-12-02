namespace Constellation.Application.Affirmations;

using Abstractions.Messaging;

public sealed record GetAffirmationQuery(
    string UserId)
    : IQuery<string>;