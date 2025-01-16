namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Absences.Plans;

using Application.Attendance.Plans.GetAttendancePlansSummary;
using Application.Attendance.Plans.GetAttendancePlansSummaryForSchool;
using Constellation.Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;
    private readonly ICurrentUserService _currentUserService;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ILogger logger,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext("APPLICATION", "Schools Portal");
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
