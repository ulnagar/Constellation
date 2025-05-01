namespace Constellation.Presentation.Schools.Areas.Schools.Pages;

using Application.Common.PresentationModels;
using Application.Domains.Attendance.Plans.Queries.CountPendingPlansForSchool;
using Application.Domains.Students.Queries.GetCurrentStudentsFromSchool;
using Constellation.Application.Domains.Students.Models;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class DashboardModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DashboardModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor,
        IServiceScopeFactory scopeFactory)
        : base(httpContextAccessor, scopeFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DashboardModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Dashboard;

    public List<StudentResponse> Students { get; set; } = new();

    public int CountPendingAttendancePlans { get; set; } = 0;

    public async Task OnGet()
    {
        if (string.IsNullOrWhiteSpace(CurrentSchoolCode))
        {
            ModalContent = new ErrorDisplay(ApplicationErrors.SchoolInvalid);

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

        _logger.Information("Requested to retrieve student list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(CurrentSchoolCode));

        if (studentsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(studentsRequest.Error);

            return;
        }

        Students = studentsRequest.Value
            .OrderBy(student => student.Grade)
            .ThenBy(student => student.Name.SortOrder)
            .ToList();
    }
}
