namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.WorkFlows.Reports;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.WorkFlows.CreateAttendanceCase;
using Application.WorkFlows.UpdateAttendanceCaseDetails;
using Constellation.Application.Domains.Attendance.Reports.Queries.GetAttendanceTrendValues;
using Core.Abstractions.Services;
using Core.Errors;
using Core.Models.Students.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class AttendanceModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AttendanceModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IAuthorizationService authService,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _authService = authService;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AttendanceModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_WorkFlows_Reports;
    [ViewData] public string PageTitle => "WorkFlow Attendance Report";

    public List<AttendanceTrend> Students { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve Attendance Values for WorkFlow creation by user {User}", _currentUserService.UserName);

        Result<List<AttendanceTrend>> studentAttendanceTrends = await _mediator.Send(new GetAttendanceTrendValuesQuery());

        if (studentAttendanceTrends.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentAttendanceTrends.Error, true)
                .Warning("Failed to retrieve Attendance Values for WorkFlow creation by user {User}", _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(studentAttendanceTrends.Error);

            return;
        }

        Students = studentAttendanceTrends.Value
            .OrderBy(entry => entry.Severity)
            .ThenBy(entry => entry.Grade)
            .ThenBy(entry => entry.Name.SortOrder)
            .ToList();
    }

    public async Task<IActionResult> OnPostCreateWorkFlows(List<WorkFlowNeeded> entries)
    {
        AuthorizationResult authorised = await _authService.AuthorizeAsync(User, AuthPolicies.CanManageWorkflows);

        if (!authorised.Succeeded)
        {
            ModalContent = new ErrorDisplay(
                DomainErrors.Auth.NotAuthorised,
                _linkGenerator.GetPathByPage("/SchoolAdmin/WorkFlows/Reports/Index", values: new { area = "Staff" }));

            return Page();
        }

        foreach (WorkFlowNeeded entry in entries)
        {
            Result result;
            switch (entry.Type)
            {
                case WorkFlowNeeded.ActionType.Create:
                    CreateAttendanceCaseCommand createCommand = new(entry.StudentId);

                    _logger
                        .ForContext(nameof(CreateAttendanceCaseCommand), createCommand, true)
                        .Information("Requested to create WorkFlow Attendance Case by user {User}", _currentUserService.UserName);

                    result = await _mediator.Send(createCommand);
                    break;
                case WorkFlowNeeded.ActionType.Update:
                    UpdateAttendanceCaseDetailsCommand updateCommand = new(entry.StudentId);

                    _logger
                        .ForContext(nameof(UpdateAttendanceCaseDetailsCommand), updateCommand, true)
                        .Information("Requested to update WorkFlow Attendance Case by user {User}", _currentUserService.UserName);

                    result = await _mediator.Send(updateCommand);
                    break;
                default:
                    result = Result.Failure(Core.Shared.Error.NullValue);
                    break;
            }

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to create WorkFlow Attendance Case by user {User}", _currentUserService.UserName);
                
                ModalContent = new ErrorDisplay(
                    result.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/WorkFlows/Reports/Index", values: new { area = "Staff" }));

                return Page();
            }
        }
        
        return RedirectToPage("/SchoolAdmin/WorkFlows/Reports/Index", new { area = "Staff" });
    }

    public class WorkFlowNeeded
    {
        public StudentId StudentId { get; set; }
        public ActionType Type { get; set; }

        public enum ActionType
        {
            Create,
            Update
        }
    }
    
}