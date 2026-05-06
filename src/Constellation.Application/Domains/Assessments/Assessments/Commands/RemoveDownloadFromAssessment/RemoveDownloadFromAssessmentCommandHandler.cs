namespace Constellation.Application.Domains.Assessments.Assessments.Commands.RemoveDownloadFromAssessment;

using Abstractions.Messaging;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Attachments.Services;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Attachments;
using Core.Models.Attachments.Enums;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class RemoveDownloadFromAssessmentCommandHandler
: ICommandHandler<RemoveDownloadFromAssessmentCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public RemoveDownloadFromAssessmentCommandHandler(
        IAssessmentRepository assessmentRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<RemoveDownloadFromAssessmentCommand>();
    }

    public async Task<Result> Handle(RemoveDownloadFromAssessmentCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(RemoveDownloadFromAssessmentCommand), request, true)
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to remove Download from Assessment");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        AssessmentDownload? download = assessment.Downloads.FirstOrDefault(entry => entry.Id == request.DownloadId);

        if (download is null)
            return Result.Success();

        assessment.RemoveDownload(download);

        Attachment? attachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.AssessmentDownload, download.Id.ToString(), cancellationToken);

        if (attachment is not null)
            _attachmentService.DeleteAttachment(attachment);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
