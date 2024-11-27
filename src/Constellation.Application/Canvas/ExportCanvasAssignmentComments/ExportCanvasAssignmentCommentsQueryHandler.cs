namespace Constellation.Application.Canvas.ExportCanvasAssignmentComments;

using Abstractions.Messaging;
using Constellation.Application.Canvas.ExportCanvasRubricResults;
using Constellation.Application.DTOs.Canvas;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models.Offerings.Errors;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Offerings.Repositories;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Offerings;
using Core.Shared;
using DTOs;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportCanvasAssignmentCommentsQueryHandler
: IQueryHandler<ExportCanvasAssignmentCommentsQuery, FileDto>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICanvasGateway _gateway;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public ExportCanvasAssignmentCommentsQueryHandler(
        IOfferingRepository offeringRepository,
        IStudentRepository studentRepository,
        ICanvasGateway gateway,
        IExcelService excelService,
        ILogger logger)
    {
        _offeringRepository = offeringRepository;
        _studentRepository = studentRepository;
        _gateway = gateway;
        _excelService = excelService;
        _logger = logger;
    }

    public async Task<Result<FileDto>> Handle(ExportCanvasAssignmentCommentsQuery request, CancellationToken cancellationToken)
    {
        if (request.OfferingId == OfferingId.Empty)
        {
            _logger
                .ForContext(nameof(ExportCanvasRubricResultsQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Failed to export Canvas Assignment Rubric data");

            return Result.Failure<FileDto>(OfferingErrors.NotFound(request.OfferingId));
        }

        Offering offering = await _offeringRepository.GetById(request.OfferingId, cancellationToken);

        if (offering is null)
        {
            _logger
                .ForContext(nameof(ExportCanvasRubricResultsQuery), request, true)
                .ForContext(nameof(Error), OfferingErrors.NotFound(request.OfferingId), true)
                .Warning("Failed to export Canvas Assignment Rubric data");

            return Result.Failure<FileDto>(OfferingErrors.NotFound(request.OfferingId));
        }

        Result<RubricEntry> rubric = await _gateway.GetCourseAssignmentDetails(request.CourseCode, request.CanvasAssignmentId, cancellationToken);
        List<CourseEnrolmentEntry> enrolments = await _gateway.GetEnrolmentsForCourse(request.CourseCode, cancellationToken);
        List<AssignmentResultEntry> submissions = await _gateway.GetCourseAssignmentSubmissions(request.CourseCode, request.CanvasAssignmentId, cancellationToken);

        List<Student> students = await _studentRepository.GetCurrentEnrolmentsForCourse(offering.CourseId, cancellationToken);

        MemoryStream stream = await _excelService.CreateCanvasAssignmentCommentExport(enrolments, submissions, students, cancellationToken);

        FileDto result = new FileDto()
        {
            FileData = stream.ToArray(),
            FileName = $"Canvas Comments - {request.CanvasAssignmentName}.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return result;
    }
}
