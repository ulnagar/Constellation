namespace Constellation.Application.Domains.Attachments.Queries.GetTemporaryFiles;

using Abstractions.Messaging;
using Core.Models.Attachments;
using Core.Models.Attachments.Repository;
using Core.Models.Reports;
using Core.Models.Reports.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTemporaryFilesQueryHandler
: IQueryHandler<GetTemporaryFilesQuery, List<ExternalReportTemporaryFileResponse>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetTemporaryFilesQueryHandler(
        IReportRepository reportRepository,
        IAttachmentRepository attachmentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _reportRepository = reportRepository;
        _attachmentRepository = attachmentRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetTemporaryFilesQuery>();
    }

    public async Task<Result<List<ExternalReportTemporaryFileResponse>>> Handle(GetTemporaryFilesQuery request, CancellationToken cancellationToken)
    {
        List<ExternalReportTemporaryFileResponse> responses = new();
        
        List<TempExternalReport> existingFiles = await _reportRepository.GetTempExternalReports(cancellationToken);

        if (existingFiles.Count == 0)
            return responses;

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        if (students.Count == 0)
            return responses;

        List<Attachment> attachments = await _attachmentRepository.GetTempFiles(cancellationToken);

        foreach (TempExternalReport report in existingFiles)
        {
            Student? student = report.StudentId != StudentId.Empty
                ? students.FirstOrDefault(entry => entry.Id == report.StudentId)
                : null;

            Attachment? attachment = attachments.FirstOrDefault(entry => entry.LinkId == report.Id.ToString());

            if (attachment is null)
                continue;

            responses.Add(new(
                report.Id,
                attachment.Name,
                report.StudentId,
                student?.Name,
                report.Type,
                report.IssuedDate));
        }

        return responses;
    }
}
