namespace Constellation.Application.Attendance.GetAttendanceDataForYearFromSentral;

using Abstractions.Messaging;
using Constellation.Core.Models.Attendance;
using Constellation.Core.Models.Attendance.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Clock;
using Core.Shared;
using GetAttendanceDataFromSentral;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendanceDataForYearFromSentralCommandHandler
: ICommandHandler<GetAttendanceDataForYearFromSentralCommand>
{
    private readonly ISentralGateway _gateway;
    private readonly IDateTimeProvider _dateTime;
    private readonly IExcelService _excelService;
    private readonly IStudentRepository _studentRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetAttendanceDataForYearFromSentralCommandHandler(
        ISentralGateway gateway,
        IDateTimeProvider dateTime,
        IExcelService excelService,
        IStudentRepository studentRepository,
        IAttendanceRepository attendanceRepository,
        IUnitOfWork unitOfWork)
    {
        _gateway = gateway;
        _dateTime = dateTime;
        _excelService = excelService;
        _studentRepository = studentRepository;
        _attendanceRepository = attendanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(GetAttendanceDataForYearFromSentralCommand request, CancellationToken cancellationToken)
    {
        bool stopProcessing = false;

        string year = _dateTime.CurrentYear.ToString();

        for (int term = 1; term < 5; term++)
        {
            if (stopProcessing)
                continue;

            for (int week = 1; week < 12; week++)
            {
                if (stopProcessing)
                    continue;

                Result<(DateOnly StartDate, DateOnly EndDate)> dates = await _gateway.GetDatesForWeek(year, term.ToString(), week.ToString());
                if (dates.IsFailure)
                    continue;

                if (dates.Value.StartDate <= _dateTime.Today && dates.Value.EndDate >= _dateTime.Today)
                {
                    stopProcessing = true;
                    continue;
                }

                SystemAttendanceData data = await _gateway.GetAttendancePercentages(term.ToString(), week.ToString(), year, dates.Value.StartDate, dates.Value.EndDate);
                if (data is null)
                    continue;

                List<StudentAttendanceData> attendanceData = new();

                attendanceData = _excelService.ExtractPerDayYearToDateAttendanceData(data, attendanceData);
                attendanceData = _excelService.ExtractPerDayWeekAttendanceData(data, attendanceData);
                attendanceData = _excelService.ExtractPerMinuteYearToDateAttendanceData(data, attendanceData);
                attendanceData = _excelService.ExtractPerMinuteWeekAttendanceData(data, attendanceData);

                foreach (StudentAttendanceData entry in attendanceData)
                {
                    Student student = await _studentRepository.GetById(entry.StudentId, cancellationToken);

                    if (student is null)
                        continue;

                    SchoolEnrolment? enrolment = student.CurrentEnrolment;

                    if (enrolment is null)
                        continue;

                    Result<AttendanceValue> modelRequest = AttendanceValue.Create(
                        student.Id,
                        enrolment.Grade,
                        dates.Value.StartDate,
                        dates.Value.EndDate,
                        $"Term {term}, Week {week}, {year}",
                        entry.MinuteYTD,
                        entry.MinuteWeek,
                        entry.DayYTD,
                        entry.DayWeek);

                    if (modelRequest.IsFailure)
                        continue;

                    _attendanceRepository.Insert(modelRequest.Value);
                }

                await _unitOfWork.CompleteAsync(cancellationToken);
            }
        }

        return Result.Success();
    }
}
