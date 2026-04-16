namespace Constellation.Application.Domains.Assessments.Archive.Queries.GetCurrentAssignmentsListing;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentAssignmentsListingQuery(
    GetCurrentAssignmentsListingQuery.Filter Selected = GetCurrentAssignmentsListingQuery.Filter.Current)
    : IQuery<List<CurrentAssignmentSummaryResponse>>
{
    public enum Filter
    {
        All,
        Current,
        Expired
    }
}
