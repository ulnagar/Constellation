namespace Constellation.Application.Domains.StudentReports.Commands.CreateNewStudentReport;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models.Attachments;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.Services;
using Core.Models.Reports;
using Core.Models.Reports.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

public class CreateNewStudentReportCommandHandler 
    : ICommandHandler<CreateNewStudentReportCommand>
{
    private readonly IReportRepository _reportRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTime;

    public CreateNewStudentReportCommandHandler(
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

    public async Task<Result> Handle(CreateNewStudentReportCommand request, CancellationToken cancellationToken)
    {
        var report = AcademicReport.Create(
            new(),
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
