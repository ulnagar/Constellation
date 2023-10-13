namespace Constellation.Application.Reports.CreateNewStudentReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Reports;
using Constellation.Core.Shared;
using Core.Models.Attachments;
using Core.Models.Attachments.ValueObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

public class CreateNewStudentReportCommandHandler 
    : ICommandHandler<CreateNewStudentReportCommand>
{
    private readonly IAcademicReportRepository _reportRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNewStudentReportCommandHandler(
        IAcademicReportRepository reportRepository,
        IAttachmentRepository attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _attachmentRepository = attachmentRepository;
        _unitOfWork = unitOfWork;
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

        var file = new Attachment
        {
            Name = request.FileName,
            FileType = "application/pdf",
            FileData = request.FileData,
            CreatedAt = DateTime.Now,
            LinkType = AttachmentType.StudentReport,
            LinkId = report.Id.ToString()
        };

        _attachmentRepository.Insert(file);

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
