namespace Constellation.Application.Offerings.GetSessionListForOffering;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetSessionListForOfferingQuery(
    OfferingId OfferingId)
    : IQuery<List<SessionListResponse>>;