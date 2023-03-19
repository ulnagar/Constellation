namespace Constellation.Application.ClassCovers.GetCoverWithDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record GetCoverWithDetailsQuery(
    ClassCoverId Id)
    : IQuery<CoverWithDetailsResponse>;
