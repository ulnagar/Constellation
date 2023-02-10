namespace Constellation.Application.ClassCovers.GetAllCurrentAndFutureCovers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.ClassCovers.Models;
using System.Collections.Generic;

public sealed record GetAllCurrentAndFutureCoversQuery()
    : IQuery<List<CoversListResponse>>;
