namespace Constellation.Application.Reports.ReplaceStudentReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ReplaceStudentReportCommandHandler 
    : ICommandHandler<ReplaceStudentReportCommand>
{
    private readonly IAcademicReportRepository _reportRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReplaceStudentReportCommandHandler(
        IAcademicReportRepository reportRepository,
        IAttachmentRepository attachmentRepository,
        IUnitOfWork unitOfWork)
    {
        _reportRepository = reportRepository;
        _attachmentRepository = attachmentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReplaceStudentReportCommand request, CancellationToken cancellationToken)
    {
        var existingReport = await _reportRepository.GetByPublishId(request.OldPublishId, cancellationToken);

        if (existingReport is null)
            return Result.Failure(DomainErrors.Reports.AcademicReport.NotFoundByPublishId(request.OldPublishId));

        var existingFile = await _attachmentRepository.GetAcademicReportByLinkId(existingReport.Id.ToString(), cancellationToken);

        if (existingFile is null)
            return Result.Failure(DomainErrors.Documents.AcademicReport.NotFound(existingReport.Id.ToString()));

        existingReport.Update(request.NewPublishId);

        existingFile.FileData = request.FileData;
        existingFile.CreatedAt = DateTime.Now;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return Result.Success();
    }
}
