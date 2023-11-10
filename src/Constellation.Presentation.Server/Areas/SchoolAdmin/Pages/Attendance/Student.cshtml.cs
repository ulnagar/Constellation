namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Attendance;

using Constellation.Application.Students.GetStudentById;
using Constellation.Application.Students.Models;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Repositories;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

public class StudentModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAttendanceRepository _repository;
    private readonly IDateTimeProvider _dateTime;

    public StudentModel(
        ISender mediator,
        IAttendanceRepository repository,
        IDateTimeProvider dateTime)
    {
        _mediator = mediator;
        _repository = repository;
        _dateTime = dateTime;
    }

    [BindProperty(SupportsGet = true)]
    public string StudentId { get; set; }

    public StudentResponse Student { get; set; }

    public List<AttendanceValue> Points { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Result<StudentResponse> student = await _mediator.Send(new GetStudentByIdQuery(StudentId));

        if (student.IsSuccess)
            Student = student.Value;

        Points = await _repository.GetAllForStudent(_dateTime.CurrentYear, StudentId);
    }
}