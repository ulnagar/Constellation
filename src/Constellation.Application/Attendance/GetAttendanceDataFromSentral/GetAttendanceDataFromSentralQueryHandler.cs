namespace Constellation.Application.Attendance.GetAttendanceDataFromSentral;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.Attendance;
using Core.Models.Students;
using Core.Shared;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendanceDataFromSentralQueryHandler
    : IQueryHandler<GetAttendanceDataFromSentralQuery, List<AttendanceValue>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ISentralGateway _sentralGateway;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public GetAttendanceDataFromSentralQueryHandler(
        IStudentRepository studentRepository,
        ISchoolRepository schoolRepository,
        ISentralGateway sentralGateway,
        IExcelService excelService,
        ILogger logger)
    {
        _studentRepository = studentRepository;
        _schoolRepository = schoolRepository;
        _sentralGateway = sentralGateway;
        _excelService = excelService;
        _logger = logger.ForContext<GetAttendanceDataFromSentralQuery>();
    }

    public async Task<Result<List<AttendanceValue>>> Handle(GetAttendanceDataFromSentralQuery request, CancellationToken cancellationToken)
    {
        Result<(DateOnly StartDate, DateOnly EndDate)> dateResponse = await _sentralGateway.GetDatesForWeek(request.Year, request.Term, request.Week);

        if (dateResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(GetAttendanceDataFromSentralQuery), request, true)
                .ForContext(nameof(Error), dateResponse.Error, true)
                .Warning("Failed to retrieve attendance data from Sentral");

            return Result.Failure<List<AttendanceValue>>(new Error("TBC", "TBC - GetAttendanceDataFromSentralQuery:50"));
        }

        SystemAttendanceData data = await _sentralGateway.GetAttendancePercentages(request.Term, request.Week, request.Year, dateResponse.Value.StartDate, dateResponse.Value.EndDate);

        if (data is null)
        {
            _logger
                .ForContext(nameof(GetAttendanceDataFromSentralQuery), request, true)
                .Warning("Failed to retrieve attendance data from Sentral");

            return Result.Failure<List<AttendanceValue>>(new Error("TBC", "TBC - GetAttendanceDataFromSentralQuery:61"));
        }

        List<StudentAttendanceData> attendanceData = new();
        List<AttendanceValue> response = new();

        attendanceData = _excelService.ExtractPerDayYearToDateAttendanceData(data, attendanceData);
        attendanceData = _excelService.ExtractPerDayWeekAttendanceData(data, attendanceData);
        attendanceData = _excelService.ExtractPerMinuteYearToDateAttendanceData(data, attendanceData);
        attendanceData = _excelService.ExtractPerMinuteWeekAttendanceData(data, attendanceData);

        foreach (StudentAttendanceData entry in attendanceData)
        {
            Student student = await _studentRepository.GetById(entry.StudentId, cancellationToken);

            if (student is null)
                continue;

            Result<AttendanceValue> modelRequest = AttendanceValue.Create(
                student.StudentId,
                student.CurrentGrade,
                dateResponse.Value.StartDate,
                dateResponse.Value.EndDate,
                $"Term {request.Term}, Week {request.Week}, {request.Year}",
                entry.MinuteYTD,
                entry.MinuteWeek,
                entry.DayYTD,
                entry.DayWeek);

            if (modelRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(GetAttendanceDataFromSentralQuery), request, true)
                    .ForContext(nameof(Error), modelRequest.Error, true)
                    .Warning("Failed to retrieve attendance data from Sentral");

                return Result.Failure<List<AttendanceValue>>(modelRequest.Error);
            }

            response.Add(modelRequest.Value);
        }

        return response;
    }
}