namespace Constellation.Application.MandatoryTraining.GenerateStaffReport;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Attachments.Repository;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
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
    private readonly IExcelService _excelService;
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IAttachmentService _attachmentService;

    public GenerateStaffReportCommandHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ISchoolRepository schoolRepository,
        ISchoolContactRepository schoolContactRepository,
        IExcelService excelService,
        IAttachmentRepository attachmentRepository,
        IAttachmentService attachmentService)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _schoolRepository = schoolRepository;
        _schoolContactRepository = schoolContactRepository;
        _excelService = excelService;
        _attachmentRepository = attachmentRepository;
        _attachmentService = attachmentService;
    }

    public async Task<Result<ReportDto>> Handle(GenerateStaffReportCommand request, CancellationToken cancellationToken)
    {
        StaffCompletionListDto data = new();

        // - Get all modules
        List<TrainingModule> modules = await _trainingModuleRepository.GetAllCurrent(cancellationToken);

        // - Get all staff
        Staff staff = await _staffRepository.GetById(request.StaffId, cancellationToken);

        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(request.StaffId, cancellationToken);

        School school = await _schoolRepository.GetById(staff.SchoolCode, cancellationToken);

        List<SchoolContact> principals = await _schoolContactRepository.GetPrincipalsForSchool(staff.SchoolCode, cancellationToken);

        data.StaffId = request.StaffId;
        data.Name = staff.DisplayName;
        data.SchoolName = staff.School.Name;
        data.EmailAddress = staff.EmailAddress;
        data.Faculties = faculties.Select(faculty => faculty.Name).ToList();

        foreach (TrainingModule module in modules)
        {
            List<TrainingCompletion> records = module.Completions
                .Where(record =>
                    record.StaffId == staff.StaffId &&
                    !record.IsDeleted)
                .ToList();

            TrainingCompletion record = records
                .OrderByDescending(record =>
                    (record.CompletedDate.HasValue) ? record.CompletedDate.Value : record.CreatedAt)
                .FirstOrDefault();

            CompletionRecordExtendedDetailsDto entry = new();
            entry.AddModuleDetails(module);
            entry.AddStaffDetails(staff);

            foreach (Faculty faculty in faculties)
            {
                List<Staff> headTeachers = faculty
                    .Members
                    .Where(member =>
                        !member.IsDeleted &&
                        member.Role == Core.Enums.FacultyMembershipRole.Manager)
                    .Select(member => member.Staff)
                    .ToList();

                foreach (Staff headTeacher in headTeachers)
                    entry.AddHeadTeacherDetails(faculty, headTeacher);
            }

            foreach (SchoolContact principal in principals)
                entry.AddPrincipalDetails(principal, school);

            if (record is not null)
                entry.AddRecordDetails(record);

            entry.CalculateExpiry();

            data.Modules.Add(entry);
        }

        // Generate CSV/XLSX file
        MemoryStream fileData = await _excelService.CreateTrainingModuleStaffReportFile(data);

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

        List<string> recordIds = data.Modules
            .Where(record => record.RecordId is not null)
            .Select(record => record.RecordId.ToString())
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
