namespace Constellation.Presentation.Schools.Areas.Schools.Pages;

using Application.Common.PresentationModels;
using Application.Domains.Attendance.Plans.Queries.CountPendingPlansForSchool;
using Application.Domains.SciencePracs.Queries.CountOutstandingScienceRollsForSchool;
using Constellation.Application.Domains.AssetManagement.Stocktake.Models;
using Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetCurrentStocktakeEvents;
using Constellation.Application.Domains.Attendance.Absences.Queries.GetOutstandingAbsencesForSchool;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Absences.Identifiers;
using Core.Models.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_View_Value)]
public class DashboardModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DashboardModel(
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DashboardModel>()
            .ForSchoolPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Dashboard;

    public int CountPendingAttendancePlans { get; set; }

    public int CountOverdueSciencePracs { get; set; }

    public int CountAbsencesPendingVerification { get; set; }

    public bool CurrentStocktakePeriod { get; set; }

    public async Task OnGet()
    {
        if (CurrentSchoolCode == SchoolCode.Empty)
        {
            ModalContent = ErrorDisplay.Create(ApplicationErrors.SchoolInvalid);

            return;
        }

        Result<int> plansRequest = await _mediator.Send(new CountPendingPlansForSchoolQuery(CurrentSchoolCode));

        if (plansRequest.IsFailure)
        {
            _logger
                .Warning("Failed to retrieve count of pending Attendance Plans for school {school} by user {user}", CurrentSchoolCode, _currentUserService.UserName);
        }
        else
        {
            CountPendingAttendancePlans = plansRequest.Value;
        }

        Result<int> lessonsRequest = await _mediator.Send(new CountOutstandingScienceRollsForSchoolQuery(CurrentSchoolCode));

        if (lessonsRequest.IsFailure)
        {
            _logger
                .Warning("Failed to retrieve count of outstanding Science Prac rolls for school {school} by user {user}", CurrentSchoolCode, _currentUserService.UserName);
        }
        else
        {
            CountOverdueSciencePracs = lessonsRequest.Value;
        }

        Result<List<OutstandingAbsencesForSchoolResponse>> absencesRequest = await _mediator.Send(new GetOutstandingAbsencesForSchoolQuery(CurrentSchoolCode!));

        if (absencesRequest.IsFailure)
        {
            _logger
                .Warning("Failed to retrieve count of absences pending verification for school {school} by user {user}", CurrentSchoolCode, _currentUserService.UserName);
        }
        else
        {
            CountAbsencesPendingVerification = absencesRequest.Value
                .Count(absence => 
                    absence.AbsenceTimeframe != absence.PeriodTimeframe 
                    && absence.AbsenceResponseId != AbsenceResponseId.Empty);
        }

        Result<List<StocktakeEventResponse>> eventsRequest = await _mediator.Send(new GetCurrentStocktakeEventsQuery());

        if (eventsRequest.IsSuccess && eventsRequest.Value.Count > 0)
        {
            CurrentStocktakePeriod = true;
        }
    }
}
