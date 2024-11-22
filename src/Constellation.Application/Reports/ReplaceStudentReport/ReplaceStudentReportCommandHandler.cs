namespace Constellation.Application.Reports.ReplaceStudentReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Reports.Repositories;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Core.Models.Reports;
using Core.Models.Reports.Errors;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

public class ReplaceStudentReportCommandHandler 
    : ICommandHandler<ReplaceStudentReportCommand>
{
    private readonly IReportRepository _reportRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public ReplaceStudentReportCommandHandler(
        IReportRepository reportRepository,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTime)
    {
        _reportRepository = reportRepository;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
        _unitOfWork = unitOfWork;
        _dateTime = dateTime;
    }

    public async Task<Result> Handle(ReplaceStudentReportCommand request, CancellationToken cancellationToken)
    {
        AcademicReport existingReport = await _reportRepository.GetAcademicReportByPublishId(request.OldPublishId, cancellationToken);

        if (existingReport is null)
            return Result.Failure(AcademicReportErrors.NotFoundByPublishId(request.OldPublishId));

        Attachment existingFile = await _attachmentRepository.GetAcademicReportByLinkId(existingReport.Id.ToString(), cancellationToken);

        if (existingFile is null)
            return Result.Failure(DomainErrors.Documents.AcademicReport.NotFound(existingReport.Id.ToString()));

        existingReport.Update(request.NewPublishId);

        string fileName = existingFile.Name;

        _attachmentService.DeleteAttachment(existingFile);

        Attachment attachment = Attachment.CreateStudentReportAttachment(
            fileName,
            MediaTypeNames.Application.Pdf,
            existingReport.Id.ToString(),
            _dateTime.Now);

        Result attempt = await _attachmentService.StoreAttachmentData(
            attachment,
            request.FileData,
            true,
            cancellationToken);

        if (attempt.IsFailure)
        {
            return attempt;
        }

        _attachmentRepository.Insert(attachment);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
