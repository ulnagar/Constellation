namespace Constellation.Application.Attachments.ProcessPATReportZipFile;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Clock;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ProcessPATReportZipFileCommandHandler
: ICommandHandler<ProcessPATReportZipFileCommand, List<string>>
{
    private readonly IAttachmentService _attachmentService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ProcessPATReportZipFileCommandHandler(
        IAttachmentService attachmentService,
        IAttachmentRepository attachmentRepository,
        IStudentRepository studentRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _attachmentService = attachmentService;
        _attachmentRepository = attachmentRepository;
        _studentRepository = studentRepository;
        _dateTime = dateTime;
        _unitOfWork = unitOfWork;
        _logger = logger
            .ForContext<ProcessPATReportZipFileCommand>();
    }

    // Extract each report from the zip file
    // Match the student from the file name (if possible)
    // Save file to disk and database as Temp File

    public async Task<Result<List<string>>> Handle(ProcessPATReportZipFileCommand request, CancellationToken cancellationToken)
    {
        List<string> messages = new();

        using ZipArchive archive = new(request.ArchiveFile, ZipArchiveMode.Read);
        foreach (var entry in archive.Entries)
        {
            if (entry.Length == 0 || entry.Name.EndsWith('/'))
                continue;

            StudentId studentId = await MatchFile(entry.Name);

            Attachment tempFile = studentId == StudentId.Empty
                ? Attachment.CreateTempFileAttachment(entry.Name, MediaTypeNames.Application.Pdf, string.Empty, _dateTime.Now)
                : Attachment.CreateTempFileAttachment(entry.Name, MediaTypeNames.Application.Pdf, studentId.ToString(), _dateTime.Now);

            Stream entryData = entry.Open();
            using MemoryStream tempStream = new();
            await entryData.CopyToAsync(tempStream, cancellationToken);
            byte[] fileData = tempStream.ToArray();

            Result attempt = await _attachmentService.StoreAttachmentData(tempFile, fileData, true, cancellationToken);

            if (attempt.IsFailure)
            {
                // Log file that was not extracted
                _logger
                    .ForContext(nameof(ZipArchiveEntry), entry.Name)
                    .ForContext(nameof(Error), attempt.Error, true)
                    .Warning("Failed to extract file from archive");

                messages.Add($"Could not save file: {entry.Name}");

                continue;
            }

            _attachmentRepository.Insert(tempFile);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return messages;
    }

    private async Task<StudentId> MatchFile(string fileName)
    {
        var splitName = fileName.Split('-');

        var index = Array.IndexOf(splitName, "patm");
        if (index == -1)
            index = Array.IndexOf(splitName, "patr");
        if (index == -1)
            return StudentId.Empty;
        var names = splitName[..index];

        return await _studentRepository.GetStudentIdFromNameFragments(names);
    }
}
