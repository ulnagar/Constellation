namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Attendance;

using Constellation.Application.Attendance.GetAttendanceDataFromSentral;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Students.GetStudents;
using Constellation.Application.Students.Models;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Repositories;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAttendanceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;


    public IndexModel(
        ISender mediator,
        IAttendanceRepository repository,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public List<AttendanceValue> StudentData { get; set; } = new();
    public List<StudentResponse> Students { get; set; } = new();

    public async Task OnGetAsync()
    {
        Result<List<StudentResponse>> studentRequest = await _mediator.Send(new GetStudentsQuery());

        if (studentRequest.IsSuccess)
            Students = studentRequest.Value;

        StudentData = await _repository.GetAll();
    }

    public async Task<IActionResult> OnGetRetrieveAttendance(CancellationToken cancellationToken = default)
    {
        Result<List<AttendanceValue>> request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", "4", "1"), cancellationToken);

        if (request.IsSuccess)
        {
            _repository.Insert(request.Value);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
        
        return RedirectToPage();
    }
}
