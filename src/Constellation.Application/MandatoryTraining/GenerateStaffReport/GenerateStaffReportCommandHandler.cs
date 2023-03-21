namespace Constellation.Application.MandatoryTraining.GenerateStaffReport;

using Constellation.Application.Abstractions.Messaging;
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

internal sealed class GenerateStaffReportCommandHandler 
    : ICommandHandler<GenerateStaffReportCommand, ReportDto>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly ITrainingCompletionRepository _trainingCompletionRepository;
    private readonly IExcelService _excelService;
    private readonly IStoredFileRepository _storedFileRepository;

    public GenerateStaffReportCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ISchoolRepository schoolRepository,
        ISchoolContactRepository schoolContactRepository,
        ITrainingCompletionRepository trainingCompletionRepository,
        IExcelService excelService,
        IStoredFileRepository storedFileRepository)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _schoolRepository = schoolRepository;
        _schoolContactRepository = schoolContactRepository;
        _trainingCompletionRepository = trainingCompletionRepository;
        _excelService = excelService;
        _storedFileRepository = storedFileRepository;
    }

    public async Task<Result<ReportDto>> Handle(GenerateStaffReportCommand request, CancellationToken cancellationToken)
    {
        var data = new StaffCompletionListDto();

        // - Get all modules
        var modules = await _trainingModuleRepository.GetCurrentModules(cancellationToken);

        // - Get all staff
        var staff = await _staffRepository.GetById(request.StaffId, cancellationToken);

        var faculties = await _facultyRepository.GetCurrentForStaffMember(request.StaffId, cancellationToken);

        var school = await _schoolRepository.GetById(staff.SchoolCode, cancellationToken);

        var principals = await _schoolContactRepository.GetPrincipalsForSchool(staff.SchoolCode, cancellationToken);

        // - Get all completions
        var records = await _trainingCompletionRepository.GetCurrentForStaffMember(request.StaffId, cancellationToken);

        data.StaffId = request.StaffId;
        data.Name = staff.DisplayName;
        data.SchoolName = staff.School.Name;
        data.EmailAddress = staff.EmailAddress;
        data.Faculties = faculties.Select(faculty => faculty.Name).ToList();

        // - Build a collection of completions by each staff member for each module
        foreach (var record in records)
        {
            var entry = new CompletionRecordExtendedDetailsDto();
            entry.AddRecordDetails(record);

            entry.AddModuleDetails(record.Module);

            entry.AddStaffDetails(staff);

            foreach (var faculty in faculties)
            {
                var headTeachers = faculty
                    .Members
                    .Where(member =>
                        !member.IsDeleted &&
                        member.Role == Core.Enums.FacultyMembershipRole.Manager)
                    .Select(member => member.Staff)
                    .ToList();

                foreach (var headTeacher in headTeachers)
                    entry.AddHeadTeacherDetails(faculty, headTeacher);
            }

            foreach (var principal in principals)
                entry.AddPrincipalDetails(principal, school);

            data.Modules.Add(entry);
        }

        // - If a staff member has not completed a module, create a blank entry for them
        foreach (var module in modules)
        {
            var existingRecord = data.Modules.Any(record => record.ModuleId == module.Id && record.StaffId == staff.StaffId);

            if (!existingRecord)
            {
                var entry = new CompletionRecordExtendedDetailsDto();
                entry.AddModuleDetails(module);

                entry.AddStaffDetails(staff);

                entry.RecordEffectiveDate = null;
                entry.RecordNotRequired = false;

                data.Modules.Add(entry);
            }
        }

        // - Remove all superceded entries
        foreach (var record in data.Modules)
        {
            var duplicates = data.Modules.Where(entry =>
                    entry.ModuleId == record.ModuleId &&
                    entry.StaffId == record.StaffId &&
                    entry.RecordId != record.RecordId)
                .ToList();

            if (duplicates.All(entry => entry.RecordEffectiveDate < record.RecordEffectiveDate))
            {
                record.IsLatest = true;
                record.CalculateExpiry();
            }
        }

        // Generate CSV/XLSX file
        var fileData = await _excelService.CreateTrainingModuleStaffReportFile(data);

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

        var certificates = await _storedFileRepository.GetTrainingCertificatesFromList(data.Modules.Select(record => record.RecordId.ToString()).ToList(), cancellationToken);

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
