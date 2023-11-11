namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Compliance.Attendance;

using Constellation.Application.Attendance.GetAttendanceDataFromSentral;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Auth;
using Constellation.Application.Students.GetStudents;
using Constellation.Application.Students.Models;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Repositories;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
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

    [ViewData] public string ActivePage { get; set; } = CompliancePages.Attendance_Index;

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
            foreach (var entry in request.Value)
                _repository.Insert(entry);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
        
        return RedirectToPage();
    }
}
