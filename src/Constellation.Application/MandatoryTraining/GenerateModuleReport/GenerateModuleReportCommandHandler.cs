namespace Constellation.Application.MandatoryTraining.GenerateModuleReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
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
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly IExcelService _excelService;

    public GenerateModuleReportCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        ITrainingCompletionRepository trainingCompletionRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        IStoredFileRepository storedFileRepository,
        IExcelService excelService)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _trainingCompletionRepository = trainingCompletionRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _storedFileRepository = storedFileRepository;
        _excelService = excelService;
    }

    public async Task<Result<ReportDto>> Handle(GenerateModuleReportCommand request, CancellationToken cancellationToken)
    {
        ModuleDetailsDto data = new();

        // Get info from database
        var module = await _trainingModuleRepository.GetById(request.Id, cancellationToken);
            
        var completions = await _trainingCompletionRepository.GetForModule(module.Id, cancellationToken);

        data.Id = module.Id;
        data.Name = module.Name;
        data.Expiry = module.Expiry.GetDisplayName();
        data.Url = module.Url;
        data.IsActive = !module.IsDeleted;

        foreach (var completion in completions.Where(record => !record.IsDeleted))
        {
            var staffMember = await _staffRepository.GetByIdWithFacultyMemberships(completion.StaffId, cancellationToken);

            var facultyIds = staffMember
                .Faculties
                .Where(member => !member.IsDeleted)
                .Select(member => member.FacultyId)
                .ToList();

            var faculties = await _facultyRepository.GetListFromIds(facultyIds, cancellationToken);

            var entry = new CompletionRecordDto
            {
                Id = completion.Id,
                ModuleId = completion.TrainingModuleId,
                ModuleName = module.Name,
                ModuleExpiry = module.Expiry,
                StaffId = completion.StaffId,
                StaffFirstName = staffMember.FirstName,
                StaffLastName = staffMember.LastName,
                StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                CompletedDate = completion.CompletedDate,
                NotRequired = completion.NotRequired,
                CreatedAt = completion.CreatedAt
            };

            data.Completions.Add(entry);
        }

        foreach (var record in data.Completions)
        {
            record.ExpiryCountdown = record.CalculateExpiry();
            record.Status = CompletionRecordDto.ExpiryStatus.Active;

            if (data.Completions.Any(other =>
                other.Id != record.Id &&
                other.ModuleId == record.ModuleId && // true
                other.StaffId == record.StaffId && // true
                (other.NotRequired && other.CreatedAt > record.CompletedDate || // false
                !other.NotRequired && !record.NotRequired && other.CompletedDate > record.CompletedDate || // false
                record.NotRequired && record.CreatedAt < other.CompletedDate))) // false
            {
                record.Status = CompletionRecordDto.ExpiryStatus.Superceded;
            }
        }

        // Remove superceded completion records
        data.Completions = data
            .Completions
            .Where(record => record.Status != CompletionRecordDto.ExpiryStatus.Superceded)
            .OrderBy(record => record.StaffLastName)
            .ToList();

        var currentStaff = await _staffRepository.GetAllActive(cancellationToken);

        foreach (var staff in currentStaff)
        {
            if (data.Completions.Any(record => record.StaffId == staff.StaffId))
                continue;

            var facultyIds = staff
                .Faculties
                .Where(member => !member.IsDeleted)
                .Select(member => member.FacultyId)
                .ToList();

            var faculties = await _facultyRepository.GetListFromIds(facultyIds, cancellationToken);

            var record = new CompletionRecordDto
            {
                StaffId = staff.StaffId,
                StaffFirstName = staff.FirstName,
                StaffLastName = staff.LastName,
                StaffFaculty = string.Join(",", faculties.Select(faculty => faculty.Name)),
                CompletedDate = null,
                ExpiryCountdown = -9999
            };

            data.Completions.Add(record);
        }

        // Remove completion records for staff who are no longer active
        var recordStaff = data.Completions.Select(record => record.StaffId).Distinct().ToList();
        foreach (var staffId in recordStaff)
        {
            if (currentStaff.All(member => member.StaffId != staffId))
            {
                data.Completions.RemoveAll(record => record.StaffId == staffId);
            }
        }

        data.Completions = data.Completions.OrderBy(record => record.StaffLastName).ToList();

        // Generate CSV/XLSX file
        var fileData = await _excelService.CreateTrainingModuleReportFile(data);

        if (!request.IncludeCertificates)
        {
            // Wrap data in return object
            var reportDto = new ReportDto
            {
                FileData = fileData.ToArray(),
                FileName = $"Mandatory Training Report - {data.Name}.xlsx",
                FileType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };

            // Return for download
            return reportDto;
        }

        var fileList = new Dictionary<string, byte[]>();

        fileList.Add($"Mandatory Training Report - {data.Name}.xlsx", fileData.ToArray());

        var certificates = await _storedFileRepository.GetTrainingCertificatesFromList(data.Completions.Select(record => record.Id.ToString()).ToList(), cancellationToken);
            
        foreach (var certificate in certificates)
        {
            fileList.Add(certificate.Name, certificate.FileData);
        }

        // Create ZIP file
        using var memoryStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
        {
            foreach (var file in fileList)
            {
                var zipArchiveEntry = zipArchive.CreateEntry(file.Key);
                using var streamWriter = new StreamWriter(zipArchiveEntry.Open());
                streamWriter.BaseStream.Write(file.Value, 0, file.Value.Length);
            }
        }

        var zipFile = new ReportDto
        {
            FileData = memoryStream.ToArray(),
            FileName = $"Mandatory Training Export - {data.Name}.zip",
            FileType = MediaTypeNames.Application.Zip
        };

        return zipFile;
    }
}
