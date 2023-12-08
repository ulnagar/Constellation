namespace Constellation.Application.Training.Modules.GenerateModuleReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Faculty.Identifiers;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Repositories;
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
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IAttachmentService _attachmentService;
    private readonly IExcelService _excelService;
    private readonly IDateTimeProvider _dateTime;

    public GenerateModuleReportCommandHandler(
        ITrainingModuleRepository trainingRepository,
        ITrainingRoleRepository roleRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IAttachmentService attachmentService,
        IExcelService excelService,
        IDateTimeProvider dateTime)
    {
        _trainingRepository = trainingRepository;
        _roleRepository = roleRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _attachmentService = attachmentService;
        _excelService = excelService;
        _dateTime = dateTime;
    }

    public async Task<Result<ReportDto>> Handle(GenerateModuleReportCommand request, CancellationToken cancellationToken)
    {
        ModuleDetailsDto data = new();

        // Get info from database
        TrainingModule module = await _trainingRepository.GetModuleById(request.Id, cancellationToken);

        data.Id = module.Id;
        data.Name = module.Name;
        data.Expiry = module.Expiry.GetDisplayName();
        data.Url = string.IsNullOrWhiteSpace(module.Url) ? string.Empty : module.Url;
        data.IsActive = !module.IsDeleted;

        List<Staff> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

        List<TrainingRole> roles = await _roleRepository.GetRolesForModule(module.Id, cancellationToken);
        List<string> requiredStaffIds = roles
            .SelectMany(role => role.Members)
            .Select(member => member.StaffId)
            .Distinct()
            .ToList();
        
        foreach (Staff staffMember in staffMembers)
        {
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
                StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name))
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

            if (!requiredStaffIds.Contains(staffMember.StaffId))
                entry.NotMandatory = true;

            data.Completions.Add(entry);
        }

        data.Completions = data.Completions.OrderBy(record => record.StaffLastName).ToList();

        // Generate CSV/XLSX file
        MemoryStream fileData = await _excelService.CreateTrainingModuleReportFile(data);

        if (!request.IncludeCertificates)
        {
            // Wrap data in return object
            ReportDto reportDto = new()
            {
                FileData = fileData.ToArray(),
                FileName = $"Mandatory Training Report - {data.Name}.xlsx",
                FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            // Return for download
            return reportDto;
        }

        List<AttachmentResponse> fileList = new();

        fileList.Add(new(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Mandatory Training Report - {data.Name}.xlsx",
            fileData.ToArray()));

        List<string> recordIds = data
            .Completions
            .Where(record =>
                record.ExpiryCountdown > 0 &&
                !record.NotMandatory)
            .Select(record => record.Id.ToString())
            .ToList();

        foreach (string recordId in recordIds)
        {
            Result<AttachmentResponse> attempt = await _attachmentService.GetAttachmentFile(AttachmentType.TrainingCertificate, recordId, cancellationToken);

            if (attempt.IsFailure)
            {
                continue;
            }

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
