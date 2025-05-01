namespace Constellation.Application.Domains.Training.Queries.GenerateModuleReport;

using Abstractions.Messaging;
using Core.Abstractions.Clock;
using Core.Models;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Faculties;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using Core.Shared;
using Extensions;
using Helpers;
using Interfaces.Services;
using Models;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GenerateModuleReportCommandHandler
    : ICommandHandler<GenerateModuleReportCommand, ReportDto>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IExcelService _excelService;
    private readonly IDateTimeProvider _dateTime;

    public GenerateModuleReportCommandHandler(
        ITrainingModuleRepository trainingRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IAttachmentService attachmentService,
        IExcelService excelService,
        IDateTimeProvider dateTime)
    {
        _trainingRepository = trainingRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _attachmentService = attachmentService;
        _excelService = excelService;
        _dateTime = dateTime;
    }

    public async Task<Result<ReportDto>> Handle(GenerateModuleReportCommand request, CancellationToken cancellationToken)
    {
        // Get info from database
        TrainingModule module = await _trainingRepository.GetModuleById(request.Id, cancellationToken);
        
        List<Staff> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

        List<CompletionRecordDto> completions = new();
        List<ModuleDetailsDto.Assignee> assignees = new();

        foreach (Staff staffMember in staffMembers)
        {
            if (module.Assignees.Any(entry => entry.StaffId == staffMember.StaffId))
                assignees.Add(new(
                    staffMember.StaffId,
                    staffMember.GetName()));

            List<FacultyId> facultyIds = staffMember
                .Faculties
                .Where(member => !member.IsDeleted)
                .Select(member => member.FacultyId)
                .ToList();

            List<Faculty> faculties = await _facultyRepository.GetListFromIds(facultyIds, cancellationToken);

            List<TrainingCompletion> records = module.Completions
                    .Where(record =>
                        record.StaffId == staffMember.StaffId &&
                        !record.IsDeleted)
                    .ToList();

            TrainingCompletion record = records.MaxBy(record => record.CompletedDate);

            CompletionRecordDto entry = new()
            {
                StaffId = staffMember.StaffId,
                StaffFirstName = staffMember.FirstName,
                StaffLastName = staffMember.LastName,
                StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                Mandatory = module.Assignees.Any(item => item.StaffId == staffMember.StaffId)
            };

            if (record is null)
            {
                entry.CompletedDate = null;
                entry.ExpiryCountdown = -9999;
            }
            else
            {
                entry.Id = record.Id;
                entry.ModuleId = record.TrainingModuleId;
                entry.ModuleName = module.Name;
                entry.ModuleExpiry = module.Expiry;
                entry.CompletedDate = record.CompletedDate;
                entry.CreatedAt = record.CreatedAt;
                entry.ExpiryCountdown = entry.CalculateExpiry(_dateTime);
                entry.Status = CompletionRecordDto.ExpiryStatus.Active;
            }

            completions.Add(entry);
        }

        completions = completions.OrderBy(record => record.StaffLastName).ToList();

        ModuleDetailsDto data = new(
            module.Id,
            module.Name,
            module.Expiry.GetDisplayName(),
            string.IsNullOrWhiteSpace(module.Url) ? string.Empty : module.Url,
            completions,
            !module.IsDeleted,
            assignees);

        // Generate CSV/XLSX file
        MemoryStream fileData = await _excelService.CreateTrainingModuleReportFile(data);

        if (!request.IncludeCertificates)
        {
            // Wrap data in return object
            ReportDto reportDto = new()
            {
                FileData = fileData.ToArray(),
                FileName = $"Mandatory Training Report - {data.Name}.xlsx",
                FileType = FileContentTypes.ExcelModernFile
            };

            // Return for download
            return reportDto;
        }

        List<AttachmentResponse> fileList = new();

        fileList.Add(new(
            FileContentTypes.ExcelModernFile,
            $"Mandatory Training Report - {data.Name}.xlsx",
            fileData.ToArray()));

        List<string> recordIds = data
            .Completions
            .Where(record =>
                record.ExpiryCountdown > 0 &&
                !record.Mandatory)
            .Select(record => record.Id.ToString())
            .ToList();

        foreach (string recordId in recordIds)
        {
            Result<AttachmentResponse> attempt = await _attachmentService.GetAttachmentFile(AttachmentType.TrainingCertificate, recordId, cancellationToken);

            if (attempt.IsFailure)
                continue;

            fileList.Add(attempt.Value);
        }

        // Create ZIP file
        using MemoryStream memoryStream = new MemoryStream();
        using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
        {
            foreach (AttachmentResponse file in fileList)
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(file.FileName);
                await using StreamWriter streamWriter = new StreamWriter(zipArchiveEntry.Open());
                await streamWriter.BaseStream.WriteAsync(file.FileData, 0, file.FileData.Length, cancellationToken);
            }
        }

        ReportDto zipFile = new()
        {
            FileData = memoryStream.ToArray(),
            FileName = $"Mandatory Training Export - {data.Name}.zip",
            FileType = MediaTypeNames.Application.Zip
        };

        return zipFile;
    }
}
