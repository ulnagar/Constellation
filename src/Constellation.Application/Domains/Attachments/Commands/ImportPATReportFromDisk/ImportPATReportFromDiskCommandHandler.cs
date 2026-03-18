namespace Constellation.Application.Domains.Attachments.Commands.ImportPATReportFromDisk;

using Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Attachments.Services;
using Constellation.Core.Models.Reports;
using Constellation.Core.Models.Reports.Repositories;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Attachments;
using Core.Models.Reports.Enums;
using Core.Shared;
using Interfaces.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using System.Globalization;
using System.Net.Mime;

internal sealed class ImportPATReportFromDiskCommandHandler
    : ICommandHandler<ImportPATReportFromDiskCommand>
{
    private readonly IAttachmentService _attachmentService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IReportRepository _reportRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IDateTimeProvider _dateTime;
    private readonly FileSystemGatewayConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    
    public ImportPATReportFromDiskCommandHandler(
        IAttachmentService attachmentService,
        IAttachmentRepository attachmentRepository,
        IReportRepository reportRepository,
        IStudentRepository studentRepository,
        IDateTimeProvider dateTime,
        IOptions<FileSystemGatewayConfiguration> configuration,
        IUnitOfWork unitOfWork,
        ILogger logger)
    {
        _attachmentService = attachmentService;
        _attachmentRepository = attachmentRepository;
        _reportRepository = reportRepository;
        _studentRepository = studentRepository;
        _dateTime = dateTime;
        _configuration = configuration.Value;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(ImportPATReportFromDiskCommand request, CancellationToken cancellationToken)
    {
        string rootDirectoryPath = _configuration.BaseFilePath + "\\~import";

        string[] files = Directory.GetFiles(rootDirectoryPath, "*.*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (file.Length == 0)
                continue;

            string fileName = Path.GetFileName(file);

            TempExternalReport tempReport = TempExternalReport.Create();

            StudentId studentId = await GetStudentIdFromFileName(fileName);
            tempReport.UpdateStudentId(studentId);

            ReportType reportType = GetReportTypeFromFileName(fileName);
            tempReport.UpdateReportType(reportType);

            DateOnly issuedDate = GetIssuedDateFromFileName(fileName);
            tempReport.UpdateIssuedDate(issuedDate);

            Attachment tempFile = Attachment.CreateTempFileAttachment(fileName, MediaTypeNames.Application.Pdf, tempReport.Id.ToString(), _dateTime.Now);

            byte[] fileData = await File.ReadAllBytesAsync(file, cancellationToken);

            Result attempt = await _attachmentService.StoreAttachmentData(tempFile, fileData, true, cancellationToken);

            if (attempt.IsFailure)
            {
                // Log file that was not extracted
                _logger
                    .ForContext(nameof(File), fileName)
                    .ForContext(nameof(Error), attempt.Error, true)
                    .Warning("Failed to process file from folder");

                continue;
            }

            _reportRepository.Insert(tempReport);
            _attachmentRepository.Insert(tempFile);

            File.Delete(file);

            await _unitOfWork.CompleteAsync(cancellationToken);
        }

        return Result.Success();
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

        int month = Convert.ToInt32(dateFragment[2..4], CultureInfo.InvariantCulture);
        int year = Convert.ToInt32(dateFragment[4..], CultureInfo.InvariantCulture);

        return new(year, month, 1);
    }
}
