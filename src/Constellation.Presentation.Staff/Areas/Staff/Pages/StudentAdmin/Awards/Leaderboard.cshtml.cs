namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Awards;

using Application.Common.PresentationModels;
using Constellation.Application.Awards.GetStudentAwardStatistics;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class LeaderboardModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public LeaderboardModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<LeaderboardModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Awards_Leaderboard;
    [ViewData] public string PageTitle => "Student Award Leaderboards";

    public IOrderedEnumerable<IGrouping<Grade, StudentAwardStatisticsResponse>> Entries { get; set; }
    private List<StudentAwardStatisticsResponse> Leaders { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        _logger.Information("Requested to retrieve Student Award Leaderboards by user {User}", _currentUserService.UserName);

        Result<List<StudentAwardStatisticsResponse>> statisticsRequest = await _mediator.Send(new GetStudentAwardStatisticsQuery(), cancellationToken);

        if (statisticsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), statisticsRequest.Error, true)
                .Warning("Failed to retrieve Student Award Leaderboards by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(statisticsRequest.Error);

            return;
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

        Entries = Leaders
            .GroupBy(award => award.Grade)
            .OrderBy(item => item.Key);
    }
}
