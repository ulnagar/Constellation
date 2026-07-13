namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Absences.Plans;

using Application.Domains.Attendance.Plans.Queries.GetAttendancePlansSummaryForSchool;
using Constellation.Application.Domains.Attendance.Plans.Queries.GetAttendancePlansSummary;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.SchoolsPortal_Absences_View_Value)]
public class IndexModel : BasePageModel
{
    private ISender _mediator => Mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;

    public IndexModel(
        LinkGenerator linkGenerator,
        ILogger logger,
        ICurrentUserService currentUserService) 
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForSchoolPortal();
    }

    [ViewData] public string ActivePage => Models.ActivePage.Absences;

    public List<AttendancePlanSummaryResponse> Plans { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<AttendancePlanSummaryResponse>> plans = await _mediator.Send(new GetAttendancePlansSummaryForSchoolQuery(CurrentSchoolCode));

        if (plans.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), plans.Error, true)
                .Warning("Failed to retrieve Attendance Plans for school {school} by user {user}", CurrentSchoolCode, _currentUserService.UserName);

            return;
        }

        Plans = plans.Value;
    }
}
