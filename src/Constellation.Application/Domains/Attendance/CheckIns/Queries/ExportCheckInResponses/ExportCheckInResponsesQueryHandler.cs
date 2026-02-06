namespace Constellation.Application.Domains.Attendance.CheckIns.Queries.ExportCheckInResponses;

using Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Core.Models.Attendance.Checkin;
using Constellation.Core.Models.Attendance.Repositories;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Core.Abstractions.Clock;
using Core.Errors;
using Core.Shared;
using DTOs;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

internal sealed class ExportCheckInResponsesQueryHandler
: IQueryHandler<ExportCheckInResponsesQuery, FileDto>
{
    private readonly ICheckInRepository _checkInRepository;
    private readonly IExcelService _excelService;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger _logger;

    public ExportCheckInResponsesQueryHandler(
        ICheckInRepository checkInRepository,
        IExcelService excelService,
        IDateTimeProvider dateTime,
        ILogger logger)
    {
        _checkInRepository = checkInRepository;
        _excelService = excelService;
        _dateTime = dateTime;
        _logger = logger
            .ForContext<ExportCheckInResponsesQuery>();
    }

    public async Task<Result<FileDto>> Handle(ExportCheckInResponsesQuery request, CancellationToken cancellationToken)
    {
        List<CheckInResponse> responses = await _checkInRepository.GetAll(cancellationToken);

        if (request.Filter is not null)
        {
            if (request.Filter.Grades.Count > 0)
            {
                responses = responses
                    .Where(response => request.Filter.Grades.Contains(response.Grade))
                    .ToList();
            }

            if (request.Filter.Courses.Count > 0)
            {
                responses = responses
                    .Where(response => request.Filter.Courses.Select(CourseId.FromValue).Contains(response.CourseId))
                    .ToList();
            }

            if (request.Filter.Offerings.Count > 0)
            {
                responses = responses
                    .Where(response => request.Filter.Offerings.Select(OfferingId.FromValue).Contains(response.OfferingId))
                    .ToList();
            }

            if (request.Filter.Schools.Count > 0)
            {
                responses = responses
                    .Where(response => request.Filter.Schools.Contains(response.SchoolCode))
                    .ToList();
            }

            if (request.Filter.Sentiments.Count > 0)
            {
                responses = responses
                    .Where(response => request.Filter.Sentiments.Contains(response.Sentiment))
                    .ToList();
            }
        }
        
        MemoryStream stream = await _excelService.CreateCheckInExportFile(responses, cancellationToken);

        if (stream is null)
        {
            _logger
                .ForContext(nameof(ExportCheckInResponsesQuery), request, true)
                .ForContext(nameof(Error), ApplicationErrors.ExportServiceFailed, true)
                .Warning("Failed to export Check In Responses to Excel");

            return Result.Failure<FileDto>(ApplicationErrors.ExportServiceFailed);
        }

        FileDto response = new()
        {
            FileData = stream.ToArray(),
            FileName = $"Check In Export - {_dateTime.Today:O}.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return response;
    }
}
