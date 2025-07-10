namespace Constellation.Application.Domains.Covers.Queries.GetCoverWithDetails;

using Abstractions.Messaging;
using Constellation.Core.Models.Covers.Identifiers;

public sealed record GetCoverWithDetailsQuery(
    CoverId Id)
    : IQuery<CoverWithDetailsResponse>;
