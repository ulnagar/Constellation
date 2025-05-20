namespace Constellation.Application.Domains.Edval.Queries.CountEdvalDifferences;

using Abstractions.Messaging;

public sealed record CountEdvalDifferencesQuery()
    : IQuery<(int Active, int Ignored)>;