namespace Constellation.Application.Domains.Edval.Queries.GetEdvalDifferences;

using Abstractions.Messaging;
using Core.Models.Edval;
using System.Collections.Generic;

public sealed record GetEdvalDifferencesQuery()
    : IQuery<List<Difference>>;
