namespace Constellation.Application.Domains.Offerings.Queries.GetFilteredOfferingsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Subjects.Identifiers;
using System.Collections.Generic;

public sealed record GetFilteredOfferingsForSelectionListQuery(
    List<CourseId> CourseIds)
    : IQuery<List<OfferingForSelectionList>>;