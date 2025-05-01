namespace Constellation.Application.Domains.Attachments.Queries.GetTemporaryFileById;

using Abstractions.Messaging;
using Core.Models.Attachments;
using Core.Models.Attachments.Errors;
using Core.Models.Attachments.Repository;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Reports;
using Core.Models.Reports.Errors;
using Core.Models.Reports.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTemporaryFileByIdQueryHandler
: IQueryHandler<GetTemporaryFileByIdQuery, ExternalReportTemporaryFileResponse>
{
    private readonly IReportRepository _reportRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetTemporaryFileByIdQueryHandler(
        IReportRepository reportRepository,
        IAttachmentRepository attachmentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _reportRepository = reportRepository;
        _attachmentRepository = attachmentRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetTemporaryFileByIdQuery>();
    }

    public async Task<Result<ExternalReportTemporaryFileResponse>> Handle(GetTemporaryFileByIdQuery request, CancellationToken cancellationToken)
    {
        TempExternalReport? report = await _reportRepository.GetTempExternalReportById(request.ReportId, cancellationToken);

        if (report is null)
        {
            _logger
                .ForContext(nameof(GetTemporaryFileByIdQuery), request, true)
                .ForContext(nameof(Error), ExternalReportErrors.TempReportNotFound(request.ReportId), true)
                .Warning("Failed to retrieve the Temporary External Report");

            return Result.Failure<ExternalReportTemporaryFileResponse>(ExternalReportErrors.TempReportNotFound(request.ReportId));
        }

        Student? student = await _studentRepository.GetById(report.StudentId, cancellationToken);
        
        Attachment? attachment = await _attachmentRepository.GetByTypeAndLinkId(AttachmentType.TempFile, report.Id.ToString(), cancellationToken);

        if (attachment is null)
        {
            _logger
                .ForContext(nameof(GetTemporaryFileByIdQuery), request, true)
                .ForContext(nameof(TempExternalReport), report, true)
                .ForContext(nameof(Error), AttachmentErrors.NotFound(AttachmentType.TempFile, report.Id.ToString()), true)
                .Warning("Failed to retrieve the Temporary External Report");

            return Result.Failure<ExternalReportTemporaryFileResponse>(AttachmentErrors.NotFound(AttachmentType.TempFile, report.Id.ToString()));
        }

        return new ExternalReportTemporaryFileResponse(
            report.Id,
            attachment.Name,
            report.StudentId,
            student?.Name,
            report.Type,
            report.IssuedDate);
    }
}
