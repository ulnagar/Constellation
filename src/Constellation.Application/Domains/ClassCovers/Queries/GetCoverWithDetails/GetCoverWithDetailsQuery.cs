namespace Constellation.Application.Domains.ClassCovers.Queries.GetCoverWithDetails;

using Abstractions.Messaging;
using Core.Models.Identifiers;

public sealed record GetCoverWithDetailsQuery(
    ClassCoverId Id)
    : IQuery<CoverWithDetailsResponse>;
