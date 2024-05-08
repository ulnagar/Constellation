namespace Constellation.Application.Schools.GetSchoolsForSelectionList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Schools.Models;
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