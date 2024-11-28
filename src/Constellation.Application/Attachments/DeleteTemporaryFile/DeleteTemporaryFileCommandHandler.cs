namespace Constellation.Application.Attachments.DeleteTemporaryFile;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Attachments.ValueObjects;
using Constellation.Core.Models.Reports;
using Constellation.Core.Models.Reports.Repositories;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Core.Shared;
using System.Threading;
using System.Threading.Tasks;

internal sealed class DeleteTemporaryFileCommandHandler
: ICommandHandler<DeleteTemporaryFileCommand>
{
    private readonly IReportRepository _reportRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTemporaryFileCommandHandler(
        IReportRepository reportRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteTemporaryFileCommand request, CancellationToken cancellationToken)
    {
        TempExternalReport report = await _reportRepository.GetTempExternalReportById(request.ReportId, cancellationToken);

        if (report is not null)
            _reportRepository.Remove(report);
        
        Attachment? attachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.TempFile, report.Id.ToString(), cancellationToken);
        
        if (attachment is not null)
            _attachmentService.DeleteAttachment(attachment);
        
        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
