namespace Constellation.Application.Domains.Assessments.Assessments.Queries.GetAssessmentDownload;

using Abstractions.Messaging;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Shared;
using Serilog;

internal sealed class GetAssessmentDownloadQueryHandler
: IQueryHandler<GetAssessmentDownloadQuery, AssessmentDownloadResponse>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly ILogger _logger;

    public GetAssessmentDownloadQueryHandler(
        IAssessmentRepository assessmentRepository,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _logger = logger
            .ForContext<GetAssessmentDownloadQuery>();
    }

    public async Task<Result<AssessmentDownloadResponse>> Handle(GetAssessmentDownloadQuery request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentDownloadQuery), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to retrieve Assessment Download");

            return Result.Failure<AssessmentDownloadResponse>(AssessmentErrors.NotFound(request.AssessmentId));
        }

        AssessmentDownload? download = assessment.Downloads.FirstOrDefault(entry => entry.Id == request.AssessmentDownloadId);

        if (download is null)
        {
            _logger
                .ForContext(nameof(GetAssessmentDownloadQuery), request, true)
                .ForContext(nameof(Error), AssessmentDownloadErrors.NotFound(request.AssessmentDownloadId), true)
                .Warning("Failed to retrieve Assessment Download");

            return Result.Failure<AssessmentDownloadResponse>(AssessmentDownloadErrors.NotFound(request.AssessmentDownloadId));
        }

        bool isActive = download.AvailableFrom <= DateOnly.FromDateTime(DateTime.UtcNow) && download.AvailableTo >= DateOnly.FromDateTime(DateTime.UtcNow);

        return new AssessmentDownloadResponse(
            assessment.Id,
            download.Id,
            download.Name,
            download.AvailableFrom,
            download.AvailableTo,
            download.IsRestricted,
            isActive);
    }
}
