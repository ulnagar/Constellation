namespace Constellation.Application.Domains.Covers.Queries.GetAllCurrentAndFutureCovers;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAllCurrentAndFutureCoversQuery()
    : IQuery<List<CoversListResponse>>;
