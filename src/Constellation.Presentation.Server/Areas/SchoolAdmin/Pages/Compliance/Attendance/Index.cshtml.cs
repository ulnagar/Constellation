namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Compliance.Attendance;

using Application.Attendance.GetAttendanceDataFromSentral;
using Application.Interfaces.Gateways;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Students.GetStudents;
using Constellation.Application.Students.Models;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Core.Abstractions.Clock;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAttendanceRepository _repository;
    private readonly IStudentRepository _studentRepository;
    private readonly IExcelService _excelService;
    private readonly ISentralGateway _gateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;

    public IndexModel(
        ISender mediator,
        IAttendanceRepository repository,
        IStudentRepository studentRepository,
        IExcelService excelService,
        ISentralGateway gateway,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _repository = repository;
        _studentRepository = studentRepository;
        _excelService = excelService;
        _gateway = gateway;
        _dateTime = dateTime;
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

        StudentData = await _repository.GetAllRecent();
    }

    public async Task OnGetRetrieveAttendance()
    {
        bool stopProcessing = false;

        for (var term = 1; term < 5; term++)
        {
            if (stopProcessing)
                continue;

            for (var week = 1; week < 12; week++)
            {
                if (stopProcessing)
                    continue;

                var dates = await _gateway.GetDatesForWeek("2023", term.ToString(), week.ToString());
                if (dates.IsFailure)
                {
                    continue;
                }

                if (dates.Value.StartDate <= _dateTime.Today && dates.Value.EndDate >= _dateTime.Today)
                {
                    stopProcessing = true;
                    continue;
                }

                SystemAttendanceData data = await _gateway.GetAttendancePercentages(term.ToString(), week.ToString(), "2023", dates.Value.StartDate, dates.Value.EndDate);

                if (data is null)
                    continue;

                List<StudentAttendanceData> attendanceData = new();

                attendanceData = _excelService.ExtractPerDayYearToDateAttendanceData(data, attendanceData);
                attendanceData = _excelService.ExtractPerDayWeekAttendanceData(data, attendanceData);
                attendanceData = _excelService.ExtractPerMinuteYearToDateAttendanceData(data, attendanceData);
                attendanceData = _excelService.ExtractPerMinuteWeekAttendanceData(data, attendanceData);

                foreach (StudentAttendanceData entry in attendanceData)
                {
                    Student student = await _studentRepository.GetById(entry.StudentId);

                    if (student is null)
                        continue;

                    Result<AttendanceValue> modelRequest = AttendanceValue.Create(
                        student.StudentId,
                        student.CurrentGrade,
                        dates.Value.StartDate,
                        dates.Value.EndDate,
                        $"Term {term}, Week {week}, {_dateTime.CurrentYear}",
                        entry.MinuteYTD,
                        entry.MinuteWeek,
                        entry.DayYTD,
                        entry.DayWeek);

                    if (modelRequest.IsFailure)
                        continue;

                    _repository.Insert(modelRequest.Value);
                }

                await _unitOfWork.CompleteAsync();
            }
        }
    }
}
