namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards;

using Constellation.Application.Awards.GetStudentAwardStatistics;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class LeaderboardModel : BasePageModel
{
    private readonly IMediator _mediator;

    public LeaderboardModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Leaderboard;


    [BindProperty]
    public DateOnly FromDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [BindProperty]
    public DateOnly ToDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public List<StudentAwardStatisticsResponse> Leaders { get; set; } = new();

    public bool IsFiltered { get; set; } = false;

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        Result<List<StudentAwardStatisticsResponse>> statisticsRequest = await _mediator.Send(new GetStudentAwardStatisticsQuery(), cancellationToken);

        if (statisticsRequest.IsFailure)
        {
            // Do something
        }

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            List<StudentAwardStatisticsResponse> studentWinners = statisticsRequest.Value
                .Where(student => student.Grade == grade)
                .GroupBy(student => student.AwardedAstras)
                .OrderByDescending(group => group.Key)
                .Take(5)
                .SelectMany(group => group)
                .ToList();

            Leaders.AddRange(studentWinners);
        }
    }

    public async Task OnPostFilter(CancellationToken cancellationToken = default)
    {
        Result<List<StudentAwardStatisticsResponse>> statisticsRequest = await _mediator.Send(new GetStudentAwardStatisticsQuery(FromDate, ToDate), cancellationToken);

        if (statisticsRequest.IsFailure)
        {
            // Do something
        }

        foreach (Grade grade in Enum.GetValues(typeof(Grade)))
        {
            List<StudentAwardStatisticsResponse> studentWinners = statisticsRequest.Value
                .Where(student => student.Grade == grade)
                .GroupBy(student => student.AwardedAstras)
                .OrderByDescending(group => group.Key)
                .Take(5)
                .SelectMany(group => group)
                .ToList();

            Leaders.AddRange(studentWinners);
        }

        IsFiltered = true;
    }
}
