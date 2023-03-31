namespace Constellation.Application.Assignments.GetCurrentAssignmentsListing;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentAssignmentsListingQuery()
    : IQuery<List<CurrentAssignmentSummaryResponse>>;
