namespace Constellation.Application.Domains.Tutorials.Queries.GetAllTutorials;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllTutorialsQuery(
    GetAllTutorialsQuery.FilterEnum Filter)
: IQuery<List<TutorialSummaryResponse>>
{
    public enum FilterEnum
    {
        All,
        Active,
        Inactive
    }
}
