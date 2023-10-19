﻿namespace Constellation.Application.Attendance.GetAttendanceDataFromSentral;

using Abstractions.Messaging;
using Core.Models;
using Core.Shared;
using Interfaces.Gateways;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAttendanceDataFromSentralQueryHandler
    : IQueryHandler<GetAttendanceDataFromSentralQuery, List<StudentAttendanceData>>
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

    public async Task<Result<List<StudentAttendanceData>>> Handle(GetAttendanceDataFromSentralQuery request, CancellationToken cancellationToken)
    {
        SystemAttendanceData data = await _sentralGateway.GetAttendancePercentages(request.Term, request.Week, request.Year);

        if (data is null)
        {
            _logger
                .ForContext(nameof(GetAttendanceDataFromSentralQuery), request, true)
                .Warning("Failed to retrieve attendance data from Sentral");

            return Result.Failure<List<StudentAttendanceData>>(new Error("TBC", "TBC - GetAttendanceDataFromSentralQuery:39"));
        }

        List<StudentAttendanceData> response = new();

        response = _excelService.ExtractYTDDayData(data, response);
        response = _excelService.ExtractYTDMinuteData(data, response);
        response = _excelService.ExtractFNDayData(data, response);
        response = _excelService.ExtractFNMinuteData(data, response);

        foreach (StudentAttendanceData entry in response)
        {
            Student student = await _studentRepository.GetById(entry.StudentId, cancellationToken);

            if (student is null)
                continue;

            School school = await _schoolRepository.GetById(student.SchoolCode, cancellationToken);

            entry.Name = student.DisplayName;
            entry.Grade = student.CurrentGrade;
            entry.SchoolName = school?.Name;
        }

        return response;
    }
}