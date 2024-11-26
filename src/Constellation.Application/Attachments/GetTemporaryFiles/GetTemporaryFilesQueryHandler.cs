namespace Constellation.Application.Attachments.GetTemporaryFiles;

using Abstractions.Messaging;
using Core.Models.Attachments;
using Core.Models.Attachments.Repository;
using Core.Models.Reports.Enums;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetTemporaryFilesQueryHandler
: IQueryHandler<GetTemporaryFilesQuery, List<ExternalReportTemporaryFileResponse>>
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetTemporaryFilesQueryHandler(
        IAttachmentRepository attachmentRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _attachmentRepository = attachmentRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetTemporaryFilesQuery>();
    }

    public async Task<Result<List<ExternalReportTemporaryFileResponse>>> Handle(GetTemporaryFilesQuery request, CancellationToken cancellationToken)
    {
        List<ExternalReportTemporaryFileResponse> responses = new();
        
        List<Attachment> existingFiles = await _attachmentRepository.GetTempFiles(cancellationToken);

        if (existingFiles.Count == 0)
            return responses;

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        if (students.Count == 0)
            return responses;

        foreach (Attachment attachment in existingFiles)
        {
            StudentId studentId = !string.IsNullOrWhiteSpace(attachment.LinkId)
                ? StudentId.FromValue(new Guid(attachment.LinkId))
                : StudentId.Empty;

            Student? student = studentId != StudentId.Empty
                ? students.FirstOrDefault(entry => entry.Id == studentId)
                : null;

            ReportType type = MatchReportFromFileName(attachment.Name);

            responses.Add(new(
                attachment.Id,
                attachment.Name,
                studentId,
                student?.Name,
                type));
        }

        return responses;
    }

    private static ReportType MatchReportFromFileName(string fileName)
    {
        string[] splitName = fileName.Split('-');

        int index = Array.IndexOf(splitName, "patm");
        if (index != -1)
            return ReportType.PATM;

        index = Array.IndexOf(splitName, "patr");
        if (index != -1)
            return ReportType.PATR;

        return null;
    }
}
