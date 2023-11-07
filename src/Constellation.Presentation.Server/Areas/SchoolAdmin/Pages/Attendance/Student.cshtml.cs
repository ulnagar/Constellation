namespace Constellation.Presentation.Server.Areas.Test.Pages;

using BaseModels;
using Core.Models.Attendance;
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

    public List<AttendanceValue> Points { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Points = _service.GetDataForStudent(StudentId);
    }
}