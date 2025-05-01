namespace Constellation.Application.Domains.Assignments.Queries.GetCurrentAssignmentsListing;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentAssignmentsListingQuery()
    : IQuery<List<CurrentAssignmentSummaryResponse>>;
