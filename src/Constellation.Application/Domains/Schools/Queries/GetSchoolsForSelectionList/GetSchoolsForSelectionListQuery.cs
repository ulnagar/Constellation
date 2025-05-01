namespace Constellation.Application.Domains.Schools.Queries.GetSchoolsForSelectionList;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetSchoolsForSelectionListQuery(
    GetSchoolsForSelectionListQuery.SchoolsFilter Filter = GetSchoolsForSelectionListQuery.SchoolsFilter.All)
    : IQuery<List<SchoolSelectionListResponse>>
{
    public enum SchoolsFilter
    {
        All,
        PartnerSchools,
        WithStudents
    }
}