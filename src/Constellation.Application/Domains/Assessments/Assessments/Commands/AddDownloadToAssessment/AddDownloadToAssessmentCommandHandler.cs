namespace Constellation.Application.Domains.Assessments.Assessments.Commands.AddDownloadToAssessment;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Assessments;
using Core.Models.Assessments.Errors;
using Core.Models.Assessments.Repositories;
using Core.Models.Attachments;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;

internal sealed class AddDownloadToAssessmentCommandHandler
: ICommandHandler<AddDownloadToAssessmentCommand>
{
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public AddDownloadToAssessmentCommandHandler(
        IAssessmentRepository assessmentRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _assessmentRepository = assessmentRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<AddDownloadToAssessmentCommand>();
    }

    public async Task<Result> Handle(AddDownloadToAssessmentCommand request, CancellationToken cancellationToken)
    {
        Assessment? assessment = await _assessmentRepository.GetAssessmentById(request.AssessmentId, cancellationToken);

        if (assessment is null)
        {
            _logger
                .ForContext(nameof(Error), AssessmentErrors.NotFound(request.AssessmentId), true)
                .Warning("Failed to add download file to Assessment");

            return Result.Failure(AssessmentErrors.NotFound(request.AssessmentId));
        }

        AssessmentDownload download = new AssessmentDownload(
            assessment.Id,
            request.Name,
            request.AvailableFrom,
            request.AvailableTo,
            request.IsRestricted);

        assessment.AddDownload(download);

        Attachment fileEntity = Attachment.CreateAssessmentDownloadAttachment(
            request.File.FileName,
            request.File.FileType,
            download.Id.ToString(),
            _dateTime.Now);

        Result attempt = await _attachmentService.StoreAttachmentData(fileEntity, request.File.FileData, false, cancellationToken);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to add download file to Assessment");

            return Result.Failure(attempt.Error);
        }

        _attachmentRepository.Insert(fileEntity);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
