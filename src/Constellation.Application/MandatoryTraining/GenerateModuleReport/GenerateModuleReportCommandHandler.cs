namespace Constellation.Application.MandatoryTraining.GenerateModuleReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using System;
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
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IExcelService _excelService;

    public GenerateModuleReportCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IStoredFileRepository storedFileRepository,
        IExcelService excelService)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _storedFileRepository = storedFileRepository;
        _excelService = excelService;
    }

    public async Task<Result<ReportDto>> Handle(GenerateModuleReportCommand request, CancellationToken cancellationToken)
    {
        ModuleDetailsDto data = new();

        // Get info from database
        TrainingModule module = await _trainingModuleRepository.GetById(request.Id, cancellationToken);
            
        data.Id = module.Id;
        data.Name = module.Name;
        data.Expiry = module.Expiry.GetDisplayName();
        data.Url = (string.IsNullOrWhiteSpace(module.Url) ? string.Empty : module.Url);
        data.IsActive = !module.IsDeleted;

        List<Staff> staffMembers = await _staffRepository.GetAllActive(cancellationToken);

        foreach (Staff staffMember in staffMembers)
        {
            List<Guid> facultyIds = staffMember
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

            TrainingCompletion record = records
                .OrderByDescending(record =>
                    (record.CompletedDate.HasValue) ? record.CompletedDate.Value : record.CreatedAt)
                .FirstOrDefault();

            if (record is null)
            {
                CompletionRecordDto entry = new() 
                {
                    StaffId = staffMember.StaffId,
                    StaffFirstName = staffMember.FirstName,
                    StaffLastName = staffMember.LastName,
                    StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                    CompletedDate = null,
                    ExpiryCountdown = -9999
                };

                data.Completions.Add(entry);
            }
            else
            {
                CompletionRecordDto entry = new()
                {
                    Id = record.Id,
                    ModuleId = record.TrainingModuleId,
                    ModuleName = module.Name,
                    ModuleExpiry = module.Expiry,
                    StaffId = record.StaffId,
                    StaffFirstName = staffMember.FirstName,
                    StaffLastName = staffMember.LastName,
                    StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                    CompletedDate = record.CompletedDate,
                    NotRequired = record.NotRequired,
                    CreatedAt = record.CreatedAt
                };

                entry.ExpiryCountdown = entry.CalculateExpiry();
                entry.Status = CompletionRecordDto.ExpiryStatus.Active;

                data.Completions.Add(entry);
            }
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

        Dictionary<string, byte[]> fileList = new();

        fileList.Add($"Mandatory Training Report - {data.Name}.xlsx", fileData.ToArray());

        List<string> recordIds = data
            .Completions
            .Where(record => 
                record.ExpiryCountdown > 0 && 
                !record.NotRequired)
            .Select(record => record.Id.ToString())
            .ToList();

        List<StoredFile> certificates = await _storedFileRepository.GetTrainingCertificatesFromList(recordIds, cancellationToken);
            
        foreach (StoredFile certificate in certificates)
            fileList.Add(certificate.Name, certificate.FileData);

        // Create ZIP file
        using MemoryStream memoryStream = new MemoryStream();
        using (ZipArchive zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
        {
            foreach (KeyValuePair<string, byte[]> file in fileList)
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(file.Key);
                using StreamWriter streamWriter = new StreamWriter(zipArchiveEntry.Open());
                streamWriter.BaseStream.Write(file.Value, 0, file.Value.Length);
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
