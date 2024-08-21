namespace Constellation.Application.Canvas.ExportCanvasRubricResults;

using Abstractions.Messaging;
using Core.Models.Canvas.Models;
using Core.Models.Offerings;
using Core.Models.Offerings.Errors;
using Core.Models.Offerings.Identifiers;
using Core.Models.Offerings.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using DTOs;
using DTOs.Canvas;
using Helpers;
using Interfaces.Gateways;
using Interfaces.Services;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ExportCanvasRubricResultsQueryHandler
: IQueryHandler<ExportCanvasRubricResultsQuery, FileDto>
{
    private readonly IOfferingRepository _offeringRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICanvasGateway _gateway;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public ExportCanvasRubricResultsQueryHandler(
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
        _logger = logger
            .ForContext<ExportCanvasRubricResultsQuery>();
    }
    
    public async Task<Result<FileDto>> Handle(ExportCanvasRubricResultsQuery request, CancellationToken cancellationToken)
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

        CanvasCourseCode courseCode = CanvasCourseCode.FromOffering(offering);
        
        RubricEntry rubric = await _gateway.GetCourseAssignmentDetails(courseCode, request.CanvasAssignmentId, cancellationToken);

        if (rubric is null)
        {
            _logger
                .ForContext(nameof(ExportCanvasRubricResultsQuery), request, true)
                .ForContext(nameof(Error), CanvasErrors.Rubric.NotFound, true)
                .Warning("Failed to export Canvas Assignment Rubric data");

            return Result.Failure<FileDto>(CanvasErrors.Rubric.NotFound);
        }

        List<CourseEnrolmentEntry> enrolments = await _gateway.GetEnrolmentsForCourse(courseCode, cancellationToken);
        List<AssignmentResultEntry> submissions = await _gateway.GetCourseAssignmentSubmissions(courseCode, request.CanvasAssignmentId, cancellationToken);

        List<Student> students = await _studentRepository.GetCurrentEnrolmentsForCourse(offering.CourseId, cancellationToken);

        MemoryStream stream = await _excelService.CreateCanvasRubricResultExport(rubric, enrolments, submissions, students, cancellationToken);

        FileDto result = new FileDto()
        {
            FileData = stream.ToArray(),
            FileName = $"Canvas Results - {request.CanvasAssignmentName}.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };

        return result;
    }
}
