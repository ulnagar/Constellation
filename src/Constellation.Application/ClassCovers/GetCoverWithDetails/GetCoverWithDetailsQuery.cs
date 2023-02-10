namespace Constellation.Application.ClassCovers.GetCoverWithDetails;

using Constellation.Application.Abstractions.Messaging;
using System;

public sealed record GetCoverWithDetailsQuery(
    Guid Id)
    : IQuery<CoverWithDetailsResponse>;
