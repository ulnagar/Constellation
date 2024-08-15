namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards;

using Application.Awards.Enums;
using Application.Common.PresentationModels;
using Application.Students.AuditAwardTallyValues;
using Constellation.Application.Awards.GetStudentAwardStatistics;
using Constellation.Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class ChangesModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public ChangesModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<ChangesModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Changes;
    [ViewData] public string PageTitle => "Award Changes";

    [BindProperty(SupportsGet = true)]
    public AwardsFilter Filter { get; set; } = AwardsFilter.All;

    public List<StudentAwardStatisticsResponse> AwardStatistics { get; set; } = new();

    public async Task OnGet(CancellationToken cancellationToken = default)
    {
        _logger
            .Information("Requested to retrieve list of Awards changes for user {User}", _currentUserService.UserName);

        Result<List<StudentAwardStatisticsResponse>> statisticsRequest = await _mediator.Send(new GetStudentAwardStatisticsQuery(), cancellationToken);

        if (statisticsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), statisticsRequest.Error, true)
                .Warning("Failed to retrieve list of Awards changes for user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(statisticsRequest.Error);

            return;
        }

        AwardStatistics = Filter switch
        {
            AwardsFilter.All => statisticsRequest.Value.Where(entry =>
                    entry.AwardedStellars != entry.CalculatedStellars ||
                    entry.AwardedGalaxies != entry.CalculatedGalaxies ||
                    entry.AwardedUniversals != entry.CalculatedUniversals)
                .ToList(),
            AwardsFilter.Additions => statisticsRequest.Value.Where(entry =>
                    entry.AwardedStellars < entry.CalculatedStellars ||
                    entry.AwardedGalaxies < entry.CalculatedGalaxies ||
                    entry.AwardedUniversals < entry.CalculatedUniversals)
                .ToList(),
            AwardsFilter.Overages => statisticsRequest.Value.Where(entry =>
                    entry.AwardedStellars > entry.CalculatedStellars ||
                    entry.AwardedGalaxies > entry.CalculatedGalaxies ||
                    entry.AwardedUniversals > entry.CalculatedUniversals)
                .ToList(),
            _ => statisticsRequest.Value.Where(entry =>
                    entry.AwardedStellars != entry.CalculatedStellars ||
                    entry.AwardedGalaxies != entry.CalculatedGalaxies ||
                    entry.AwardedUniversals != entry.CalculatedUniversals)
                .ToList()
        };
    }

    public async Task<IActionResult> OnGetAuditValues()
    {
        _logger.Information("Requested to trigger Award Tally Audit by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(new AuditAwardTallyValuesCommand());

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to complete Award Tally Audit by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            return Page();
        }

        return RedirectToPage();
    }
}
