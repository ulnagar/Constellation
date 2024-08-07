namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Compliance.Attendance;

using Application.Attendance.GetAttendanceValuesForStudent;
using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Students.GetStudentById;
using Constellation.Application.Students.Models;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Compliance_Attendance;

    [BindProperty(SupportsGet = true)]
    public string StudentId { get; set; }

    public StudentResponse Student { get; set; }
    public List<AttendanceValue> Points { get; set; } = new();

    public async Task OnGet()
    {
        if (string.IsNullOrWhiteSpace(StudentId))
        {
            ModalContent = new ErrorDisplay(
                new("Page.Parameter.Required", "A valid StudentId must be provided"),
                _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/Attendance/Index", values: new { area = "Staff" }));

            return;
        }

        Result<StudentResponse> studentRequest = await _mediator.Send(new GetStudentByIdQuery(StudentId));

        if (studentRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                studentRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/Attendance/Index", values: new { area = "Staff" }));

            return;
        }
        
        Student = studentRequest.Value;

        Result<List<AttendanceValue>> valueRequest = await _mediator.Send(new GetAttendanceValuesForStudentQuery(Student.StudentId));

        if (valueRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                valueRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Compliance/Attendance/Index", values: new { area = "Staff" }));

            return;
        }

        Points = valueRequest.Value;
    }
}