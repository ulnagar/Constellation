namespace Constellation.Application.Attachments.ProcessPATReportZipFile;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Core.Abstractions.Clock;
using Core.Models.Attachments;
using Core.Models.Attachments.Services;
using Core.Models.Reports;
using Core.Models.Reports.Enums;
using Core.Models.Reports.Repositories;
using Core.Shared;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class ProcessPATReportZipFileCommandHandler
: ICommandHandler<ProcessPATReportZipFileCommand, List<string>>
{
    private readonly IAttachmentService _attachmentService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;

    public ProcessPATReportZipFileCommandHandler(
        IAttachmentService attachmentService,
        IAttachmentRepository attachmentRepository,
        IReportRepository reportRepository,
        IStudentRepository studentRepository,
        IDateTimeProvider dateTime,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _attachmentService = attachmentService;
        _attachmentRepository = attachmentRepository;
        _reportRepository = reportRepository;
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
            
            TempExternalReport tempReport = TempExternalReport.Create();
            
            StudentId studentId = await GetStudentIdFromFileName(entry.Name);
            tempReport.UpdateStudentId(studentId);

            ReportType reportType = GetReportTypeFromFileName(entry.Name);
            tempReport.UpdateReportType(reportType);

            DateOnly issuedDate = GetIssuedDateFromFileName(entry.Name);
            tempReport.UpdateIssuedDate(issuedDate);

            Attachment tempFile = Attachment.CreateTempFileAttachment(entry.Name, MediaTypeNames.Application.Pdf, tempReport.Id.ToString(), _dateTime.Now);

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

            _reportRepository.Insert(tempReport);
            _attachmentRepository.Insert(tempFile);
        }

        await _unitOfWork.CompleteAsync(cancellationToken);

        return messages;
    }

    private async Task<StudentId> GetStudentIdFromFileName(string fileName)
    {
        string[] splitName = fileName.Split('-');

        int index = Array.IndexOf(splitName, "patm") != -1
            ? Array.IndexOf(splitName, "patm")
            : Array.IndexOf(splitName, "patr") != -1
                ? Array.IndexOf(splitName, "patr")
                : -1;

        if (index == -1)
            return StudentId.Empty;
        
        string[] names = splitName[..index];

        return await _studentRepository.GetStudentIdFromNameFragments(names);
    }

    private static ReportType GetReportTypeFromFileName(string fileName)
    {
        string[] splitName = fileName.Split('-');

        return Array.IndexOf(splitName, "patm") != -1
            ? ReportType.PATM
            : Array.IndexOf(splitName, "patr") != -1
                ? ReportType.PATR
                : ReportType.Unknown;
    }

    private static DateOnly GetIssuedDateFromFileName(string fileName)
    {
        string[] splitName = fileName.Split('-');

        string dateFragment = splitName[^2];

        if (!dateFragment.All(char.IsAsciiDigit))
            return DateOnly.MinValue;

        int month = Convert.ToInt32(dateFragment[3..4]);
        int year = Convert.ToInt32(dateFragment[4..]);

        return new(year, month, 1);
    }
}
