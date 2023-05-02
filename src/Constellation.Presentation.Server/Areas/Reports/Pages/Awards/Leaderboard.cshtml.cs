namespace Constellation.Presentation.Server.Areas.Reports.Pages.Awards;

using Constellation.Application.Awards.GetStudentAwardStatistics;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class LeaderboardModel : BasePageModel
{
    private readonly IMediator _mediator;

    public LeaderboardModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    public List<StudentAwardStatisticsResponse> Leaders { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        await GetClasses(_mediator);

        var statisticsRequest = await _mediator.Send(new GetStudentAwardStatisticsQuery(), cancellationToken);

        if (statisticsRequest.IsFailure)
        {
            // Do something
        }

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            var studentWinners = statisticsRequest.Value
                .Where(student => student.Grade == grade)
                .GroupBy(student => student.AwardedAstras)
                .OrderByDescending(group => group.Key)
                .Take(5)
                .SelectMany(group => group)
                .ToList();

            Leaders.AddRange(studentWinners);
        }
    }
}
