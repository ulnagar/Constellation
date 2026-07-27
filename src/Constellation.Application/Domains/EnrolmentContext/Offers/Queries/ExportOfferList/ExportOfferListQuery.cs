namespace Constellation.Application.Domains.EnrolmentContext.Offers.Queries.ExportOfferList;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Offer.Identifiers;
using System.Collections.Generic;

public sealed record ExportOfferListQuery(
    List<OfferId> OfferIds)
    : IQuery<byte[]>;