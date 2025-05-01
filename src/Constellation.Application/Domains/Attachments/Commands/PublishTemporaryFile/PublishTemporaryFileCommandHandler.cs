namespace Constellation.Application.Domains.Attachments.Commands.PublishTemporaryFile;

using Abstractions.Messaging;
using Core.Models.Attachments;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Errors;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Reports;
using Core.Models.Reports.Errors;
using Core.Models.Reports.Repositories;
using Core.Models.Students.Identifiers;
using Core.Shared;
using Interfaces.Repositories;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class PublishTemporaryFileCommandHandler
: ICommandHandler<PublishTemporaryFileCommand>
{
    private readonly IReportRepository _reportRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public PublishTemporaryFileCommandHandler(
        IReportRepository reportRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _reportRepository = reportRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(PublishTemporaryFileCommand request, CancellationToken cancellationToken)
    {
        TempExternalReport report = await _reportRepository.GetTempExternalReportById(request.ReportId, cancellationToken);

        if (report is null)
        {
            _logger
                .ForContext(nameof(PublishTemporaryFileCommand), request, true)
                .ForContext(nameof(Error), ExternalReportErrors.TempReportNotFound(request.ReportId), true)
                .Warning("Failed to publish Temporary External Report");
            
            return Result.Failure(ExternalReportErrors.TempReportNotFound(request.ReportId));
        }

        if (report.StudentId == StudentId.Empty)
        {
            _logger
                .ForContext(nameof(PublishTemporaryFileCommand), request, true)
                .ForContext(nameof(TempExternalReport), report, true)
                .ForContext(nameof(Error), ExternalReportErrors.NoLinkedStudent, true)
                .Warning("Failed to publish Temporary External Report");

            return Result.Failure(ExternalReportErrors.NoLinkedStudent);
        }
        
        Attachment? attachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.TempFile, report.Id.ToString(), cancellationToken);

        if (attachment is null)
        {
            _logger
                .ForContext(nameof(PublishTemporaryFileCommand), request, true)
                .ForContext(nameof(TempExternalReport), report, true)
                .ForContext(nameof(Error), AttachmentErrors.NotFound(AttachmentType.TempFile, report.Id.ToString()), true)
                .Warning("Failed to publish Temporary External Report");

            return Result.Failure(AttachmentErrors.NotFound(AttachmentType.TempFile, report.Id.ToString()));
        }

        Result<AttachmentResponse> fileData = await _attachmentService.GetAttachmentFile(AttachmentType.TempFile, report.Id.ToString(), cancellationToken);

        if (fileData.IsFailure)
        {
            _logger
                .ForContext(nameof(PublishTemporaryFileCommand), request, true)
                .ForContext(nameof(TempExternalReport), report, true)
                .ForContext(nameof(Error), fileData.Error, true)
                .Warning("Failed to publish Temporary External Report");

            return fileData;
        }
        
        // Convert to External Report
        ExternalReport externalReport = ExternalReport.ConvertFromTempExternalReport(report);
        Attachment newAttachment = Attachment.CreateExternalReportAttachment(attachment.Name, attachment.FileType, externalReport.Id.ToString(), attachment.CreatedAt);

        Result attempt = await _attachmentService.StoreAttachmentData(newAttachment, fileData.Value.FileData, true, cancellationToken);

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(PublishTemporaryFileCommand), request, true)
                .ForContext(nameof(TempExternalReport), report, true)
                .ForContext(nameof(Error), attempt.Error, true)
                .Warning("Failed to bulk publish Temporary External Report");

            return Result.Failure(attempt.Error);
        }

        _attachmentRepository.Insert(newAttachment);
        _reportRepository.Insert(externalReport);
        _attachmentService.DeleteAttachment(attachment);
        _reportRepository.Remove(report);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
