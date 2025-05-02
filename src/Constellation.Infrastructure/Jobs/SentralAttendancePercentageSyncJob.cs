namespace Constellation.Infrastructure.Jobs;

using Application.Interfaces.Gateways;
using Application.Interfaces.Jobs;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Constellation.Application.Domains.Attendance.Reports.Queries.GetAttendanceDataFromSentral;
using Constellation.Core.Enums;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Attendance.Repositories;
using System;
using System.Threading.Tasks;

internal sealed class SentralAttendancePercentageSyncJob : ISentralAttendancePercentageSyncJob
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISentralGateway _sentralGateway;
    private readonly IExcelService _excelService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public SentralAttendancePercentageSyncJob(
        IAttendanceRepository attendanceRepository,
        IStudentRepository studentRepository,
        IUnitOfWork unitOfWork,
        ISentralGateway sentralGateway,
        IExcelService excelService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _unitOfWork = unitOfWork;
        _sentralGateway = sentralGateway;
        _excelService = excelService;
        _dateTime = dateTime;
        _logger = logger.ForContext<ISentralAttendancePercentageSyncJob>();
    }

    public async Task StartJob(Guid jobId, CancellationToken cancellationToken)
    {
        // Figure out which period should be scanned
        Result<(SchoolWeek week, SchoolTerm term)> periodRequest = await _sentralGateway.GetWeekForDate(_dateTime.Yesterday);
        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), periodRequest.Error, true)
                .Warning("Could not sync the Attendance Percentages");

            return;
        }

        SchoolTerm term = periodRequest.Value.term;
        SchoolWeek week = periodRequest.Value.week;

        Result<(DateOnly StartDate, DateOnly EndDate)> dateRequest = await _sentralGateway.GetDatesForWeek(_dateTime.CurrentYearAsString, term, week);
        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), dateRequest.Error, true)
                .Warning("Could not sync the Attendance Percentages");

            return;
        }

        DateOnly startDate = dateRequest.Value.StartDate;
        DateOnly endDate = dateRequest.Value.EndDate;

        if (endDate > _dateTime.Today)
        {
            // This is for the current week, therefore the data will be incomplete. Cancel operation
            _logger
                .ForContext(nameof(startDate), startDate)
                .ForContext(nameof(endDate), endDate)
                .ForContext(nameof(term), term)
                .ForContext(nameof(week), week)
                .ForContext(nameof(_dateTime.Today), _dateTime.Today)
                .Warning("Could not sync the Attendance Percentages");

            return;
        }

        // Grab the file from Sentral
        SystemAttendanceData request = await _sentralGateway.GetAttendancePercentages(term, week, _dateTime.CurrentYearAsString, startDate, endDate);

        if (request is null)
        {
            _logger
                .Warning("Could not sync the Attendance Percentages");

            return;
        }

        List<StudentAttendanceData> attendanceData = new();

        attendanceData = _excelService.ExtractPerDayYearToDateAttendanceData(request, attendanceData);
        attendanceData = _excelService.ExtractPerDayWeekAttendanceData(request, attendanceData);
        attendanceData = _excelService.ExtractPerMinuteYearToDateAttendanceData(request, attendanceData);
        attendanceData = _excelService.ExtractPerMinuteWeekAttendanceData(request, attendanceData);

        foreach (StudentAttendanceData entry in attendanceData)
        {
            Student student = await _studentRepository.GetBySRN(entry.StudentReferenceNumber, cancellationToken);

            if (student is null)
                continue;

            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            Result<AttendanceValue> modelRequest = AttendanceValue.Create(
                student.Id,
                enrolment.Grade,
                startDate,
                endDate,
                $"{term}, {week}, {_dateTime.CurrentYear}",
                entry.MinuteYTD,
                entry.MinuteWeek,
                entry.DayYTD,
                entry.DayWeek);

            if (modelRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), modelRequest.Error, true)
                    .Warning("Could not sync the Attendance Percentages");

                continue;
            }

            _attendanceRepository.Insert(modelRequest.Value);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);
    }
}
