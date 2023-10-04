namespace Constellation.Application.GroupTutorials.GetAllTutorials;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllTutorialsQuery
    : IQuery<List<GroupTutorialSummaryResponse>>
{
    public FilterEnum Filter { get; set; } = FilterEnum.Active;

    public enum FilterEnum
    {
        All,
        Active,
        Inactive,
        Future
    }
}
