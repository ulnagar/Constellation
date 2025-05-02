namespace Constellation.Application.Domains.Attendance.Reports.Commands.UpdateAttendanceDataForPeriodFromSentral;

using Abstractions.Messaging;
using Core.Models.Attendance;
using Core.Models.Attendance.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class UpdateAttendanceDataForPeriodFromSentralCommandHandler
    : ICommandHandler<UpdateAttendanceDataForPeriodFromSentralCommand>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ISentralGateway _sentralGateway;
    private readonly IExcelService _excelService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public UpdateAttendanceDataForPeriodFromSentralCommandHandler(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository,
        ISentralGateway sentralGateway,
        IExcelService excelService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _sentralGateway = sentralGateway;
        _excelService = excelService;
        _unitOfWork = unitOfWork;
        _logger = logger.ForContext<UpdateAttendanceDataForPeriodFromSentralCommand>();
    }

    public async Task<Result> Handle(UpdateAttendanceDataForPeriodFromSentralCommand request, CancellationToken cancellationToken)
    {
        string periodName = $"{request.Term}, {request.Week}, {request.Year}";
        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);
        
        Result<(DateOnly StartDate, DateOnly EndDate)> dateResponse = await _sentralGateway.GetDatesForWeek(request.Year, request.Term, request.Week);

        if (dateResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateAttendanceDataForPeriodFromSentralCommand), request, true)
                .ForContext(nameof(Error), dateResponse.Error, true)
                .Warning("Failed to retrieve attendance data from Sentral");

            return Result.Failure(dateResponse.Error);
        }

        SystemAttendanceData data = await _sentralGateway.GetAttendancePercentages(request.Term, request.Week, request.Year, dateResponse.Value.StartDate, dateResponse.Value.EndDate);

        if (data is null)
        {
            _logger
                .ForContext(nameof(UpdateAttendanceDataForPeriodFromSentralCommand), request, true)
                .Warning("Failed to retrieve attendance data from Sentral");

            return Result.Failure(new Error("TBC", "TBC - UpdateAttendanceDataForPeriodFromSentralCommand:61"));
        }

        List<StudentAttendanceData> attendanceData = new();

        attendanceData = _excelService.ExtractPerDayYearToDateAttendanceData(data, attendanceData);
        attendanceData = _excelService.ExtractPerDayWeekAttendanceData(data, attendanceData);
        attendanceData = _excelService.ExtractPerMinuteYearToDateAttendanceData(data, attendanceData);
        attendanceData = _excelService.ExtractPerMinuteWeekAttendanceData(data, attendanceData);

        foreach (StudentAttendanceData entry in attendanceData)
        {
            Student student = students.FirstOrDefault(student => student.StudentReferenceNumber == entry.StudentReferenceNumber);

            if (student is null)
                continue;

            SchoolEnrolment enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            Result<AttendanceValue> modelRequest = AttendanceValue.Create(
                student.Id,
                enrolment.Grade,
                dateResponse.Value.StartDate,
                dateResponse.Value.EndDate,
                periodName,
                entry.MinuteYTD,
                entry.MinuteWeek,
                entry.DayYTD,
                entry.DayWeek);

            if (modelRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(UpdateAttendanceDataForPeriodFromSentralCommand), request, true)
                    .ForContext(nameof(Error), modelRequest.Error, true)
                    .Warning("Failed to retrieve attendance data from Sentral");

                return Result.Failure<List<AttendanceValue>>(modelRequest.Error);
            }

            _attendanceRepository.Insert(modelRequest.Value);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}