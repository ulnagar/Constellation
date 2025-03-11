namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Compliance.Attendance;

using Application.Attendance.GetAttendanceValuesForStudent;
using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Students.GetStudentById;
using Constellation.Application.Students.Models;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Students.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class StudentModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IDateTimeProvider _dateTime;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public StudentModel(
        ISender mediator,
        IDateTimeProvider dateTime,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _dateTime = dateTime;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<StudentModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Compliance_Attendance;
    [ViewData] public string PageTitle { get; set; } = "Student Attendance Details";

    [BindProperty(SupportsGet = true)]
    public StudentId StudentId { get; set; } = StudentId.Empty;

    public StudentResponse Student { get; set; }
    public List<AttendanceValue> Points { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve Attendance Details for student with id {Id} by user {User}", StudentId, _currentUserService.UserName);

        if (StudentId == StudentId.Empty)
        {
            Error error = new("Page.Parameter.Required", "A valid StudentId must be provided");

            _logger
                .ForContext(nameof(Error), error, true)
                .Warning("Failed to retrieve Attendance Details for student with id {Id} by user {User}", StudentId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/Attendance/Index", values: new { area = "Staff" }));

            return;
        }

        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(StudentId));

        if (studentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentRequest.Error, true)
                .Warning("Failed to retrieve Attendance Details for student with id {Id} by user {User}", StudentId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                studentRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/Attendance/Index", values: new { area = "Staff" }));

            return;
        }
        
        Student = studentRequest.Value;

        Result<List<AttendanceValue>> valueRequest = await _mediator.Send(new GetAttendanceValuesForStudentQuery(Student.StudentId));

        if (valueRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), valueRequest.Error, true)
                .Warning("Failed to retrieve Attendance Details for student with id {Id} by user {User}", StudentId, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                valueRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/Attendance/Index", values: new { area = "Staff" }));

            return;
        }

        Points = valueRequest.Value;
    }
}