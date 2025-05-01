namespace Constellation.Application.Domains.Attachments.Commands.DeleteTemporaryFile;

using Abstractions.Messaging;
using Core.Models.Attachments;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Reports;
using Core.Models.Reports.Repositories;
using Core.Shared;
using Interfaces.Repositories;
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
