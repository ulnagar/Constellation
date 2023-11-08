namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Students.GetStudentById;
using Application.Students.Models;
using BaseModels;
using Core.Models.Attendance;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Services;

public class StudentModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly StudentAttendanceService _service;

    public StudentModel(
        ISender mediator,
        StudentAttendanceService service)
    {
        _mediator = mediator;
        _service = service;
    }

    [BindProperty(SupportsGet = true)]
    public string StudentId { get; set; }

    public StudentResponse Student { get; set; }

    public List<AttendanceValue> Points { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(StudentId));

        if (student.IsSuccess)
            Student = student.Value;

        Points = _service.GetDataForStudent(StudentId);
    }
}