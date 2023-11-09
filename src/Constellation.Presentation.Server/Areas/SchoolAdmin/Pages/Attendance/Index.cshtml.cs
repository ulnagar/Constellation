namespace Constellation.Presentation.Server.Areas.Test.Pages;

using Application.Attendance.GetAttendanceDataFromSentral;
using Application.Interfaces.Repositories;
using Application.Students.GetCurrentStudentsWithSentralId;
using Application.Students.GetStudents;
using Application.Students.Models;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using Core.Shared;
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
        for (int term = 1; term < 4; term++)
        {
            for (int week = 1; week < 12; week++)
            {
                Result<List<AttendanceValue>> request = await _mediator.Send(new GetAttendanceDataFromSentralQuery("2023", term.ToString(), week.ToString()), cancellationToken);

                if (request.IsSuccess)
                {
                    _repository.Insert(request.Value);
                }
            }
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
        
        return RedirectToPage();
    }
}
