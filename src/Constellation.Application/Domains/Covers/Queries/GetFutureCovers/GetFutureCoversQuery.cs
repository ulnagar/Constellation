namespace Constellation.Application.Domains.Covers.Queries.GetFutureCovers;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetFutureCoversQuery()
    : IQuery<List<CoversListResponse>>;
