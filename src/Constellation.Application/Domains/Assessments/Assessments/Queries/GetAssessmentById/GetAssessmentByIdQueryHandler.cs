namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentById;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using GetCurrentAssessments;
using Models;
using Serilog;

internal sealed class GetAssessmentByIdQueryHandler
: IQueryHandler<GetAssessmentByIdQuery, AssessmentResponse>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetAssessmentByIdQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _logger = logger
            .ForContext<GetAssessmentByIdQuery>();
    }

    public async Task<Result<AssessmentResponse>> Handle(GetAssessmentByIdQuery request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.Id, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentByIdQuery), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.Id), true)
                .Warning("Failed to retrieve Assessment");

            return Result.Failure<AssessmentResponse>(AssessmentErrors.NotFound(request.Id));
        }

        return new AssessmentResponse(
            assessment.Id,
            assessment.Name,
            assessment.CourseId,
            assessment.Course,
            assessment.Grade,
            assessment.DueDate,
            assessment.AvailableFrom,
            assessment.AvailableTo);
    }
}
