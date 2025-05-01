namespace Constellation.Application.Domains.Offerings.Queries.GetSessionListForOffering;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetSessionListForOfferingQuery(
    OfferingId OfferingId)
    : IQuery<List<SessionListResponse>>;