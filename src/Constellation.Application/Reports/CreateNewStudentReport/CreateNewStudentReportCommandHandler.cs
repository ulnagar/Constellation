namespace Constellation.Application.Reports.CreateNewStudentReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Reports;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

public class CreateNewStudentReportCommandHandler 
    : ICommandHandler<CreateNewStudentReportCommand>
{
    private readonly IAcademicReportRepository _reportRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public CreateNewStudentReportCommandHandler(
        IAcademicReportRepository reportRepository,
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

    public async Task<Result> Handle(CreateNewStudentReportCommand request, CancellationToken cancellationToken)
    {
        var report = AcademicReport.Create(
            new AcademicReportId(),
            request.StudentId,
            request.PublishId,
            request.Year,
            request.ReportingPeriod);

        _reportRepository.Insert(report);

        Attachment attachment = Attachment.CreateStudentReportAttachment(
            request.FileName,
            MediaTypeNames.Application.Pdf,
            report.Id.ToString(),
            _dateTime.Now);

        Result attempt = await _attachmentService.StoreAttachmentData(
            attachment,
            request.FileData,
            false,
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
