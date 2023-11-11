namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Compliance.Attendance;

using Application.Attendance.GetAttendanceValuesForStudent;
using Application.Models.Auth;
using Constellation.Application.Students.GetStudentById;
using Constellation.Application.Students.Models;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class StudentModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IDateTimeProvider _dateTime;
    private readonly LinkGenerator _linkGenerator;

    public StudentModel(
        ISender mediator,
        IDateTimeProvider dateTime,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _dateTime = dateTime;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage { get; set; } = CompliancePages.Attendance_Student;

    [BindProperty(SupportsGet = true)]
    public string StudentId { get; set; }

    public StudentResponse Student { get; set; }
    public List<AttendanceValue> Points { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        if (string.IsNullOrWhiteSpace(StudentId))
        {
            Error = new()
            {
                Error = new("Page.Parameter.Required", "A valid StudentId must be provided"),
                RedirectPath = _linkGenerator.GetPathByPage("/Compliance/Attendance/Index", values: new { area = "SchoolAdmin" })
            };

            return;
        }

        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(StudentId));

        if (studentRequest.IsFailure)
        {
            Error = new()
            {
                Error = studentRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Compliance/Attendance/Index", values: new { area = "SchoolAdmin" })
            };

            return;
        }
        
        Student = studentRequest.Value;

        Result<List<AttendanceValue>> valueRequest = await _mediator.Send(new GetAttendanceValuesForStudentQuery(Student.StudentId));

        if (valueRequest.IsFailure)
        {
            Error = new()
            {
                Error = valueRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Compliance/Attendance/Index", values: new { area = "SchoolAdmin" })
            };

            return;
        }

        Points = valueRequest.Value;
    }
}