namespace Constellation.Application.ClassCovers.GetFutureCovers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.ClassCovers.Models;
using System.Collections.Generic;

public sealed record GetFutureCoversQuery()
    : IQuery<List<CoversListResponse>>;
