namespace Constellation.Application.Training.GenerateStaffReport;

using Abstractions.Messaging;
using Constellation.Core.Models;
using Constellation.Core.Models.SchoolContacts;
using Core.Errors;
using Core.Models.Attachments.DTOs;
using Core.Models.Attachments.Services;
using Core.Models.Attachments.ValueObjects;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.Faculties.ValueObjects;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using Core.Shared;
using Helpers;
using Interfaces.Repositories;
using Interfaces.Services;
using Models;
using Serilog;
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
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly ISchoolContactRepository _schoolContactRepository;
    private readonly IExcelService _excelService;
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger _logger;

    public GenerateStaffReportCommandHandler(
        ITrainingModuleRepository trainingRepository,
        IStaffRepository staffRepository,
        IFacultyRepository facultyRepository,
        ISchoolRepository schoolRepository,
        ISchoolContactRepository schoolContactRepository,
        IExcelService excelService,
        IAttachmentService attachmentService,
        ILogger logger)
    {
        _trainingRepository = trainingRepository;
        _staffRepository = staffRepository;
        _facultyRepository = facultyRepository;
        _schoolRepository = schoolRepository;
        _schoolContactRepository = schoolContactRepository;
        _excelService = excelService;
        _attachmentService = attachmentService;
        _logger = logger.ForContext<GenerateStaffReportCommand>();
    }

    public async Task<Result<ReportDto>> Handle(GenerateStaffReportCommand request, CancellationToken cancellationToken)
    {
        StaffCompletionListDto data = new();

        // - Get all staff
        Staff staff = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staff is null)
        {
            _logger
                .ForContext(nameof(GenerateStaffReportCommand), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(request.StaffId))
                .Warning("Failed to generate Staff Report for Mandatory Training");

            return Result.Failure<ReportDto>(DomainErrors.Partners.Staff.NotFound(request.StaffId));
        }

        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(request.StaffId, cancellationToken);

        School school = await _schoolRepository.GetById(staff.SchoolCode, cancellationToken);

        List<SchoolContact> principals = await _schoolContactRepository.GetPrincipalsForSchool(staff.SchoolCode, cancellationToken);

        data.StaffId = request.StaffId;
        data.Name = staff.DisplayName;
        data.SchoolName = staff.School.Name;
        data.EmailAddress = staff.EmailAddress;
        data.Faculties = faculties.Select(faculty => faculty.Name).ToList();

        List<TrainingModule> modules = await _trainingRepository.GetModulesByAssignee(staff.StaffId, cancellationToken);

        foreach (TrainingModule module in modules)
        {
            if (module.IsDeleted) continue;

            TrainingCompletion record = module.Completions
                .Where(record =>
                    record.StaffId == staff.StaffId &&
                    !record.IsDeleted)
                .MaxBy(record => record.CompletedDate);

            CompletionRecordExtendedDetailsDto entry = new();
            entry.AddModuleDetails(module);
            entry.AddStaffDetails(staff);

            foreach (Faculty faculty in faculties)
            {
                List<string> headTeacherIds = faculty
                    .Members
                    .Where(member =>
                        !member.IsDeleted &&
                        member.Role == FacultyMembershipRole.Manager)
                    .Select(member => member.StaffId)
                    .ToList();

                List<Staff> headTeachers = await _staffRepository
                    .GetListFromIds(headTeacherIds, cancellationToken);

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
                FileType = FileContentTypes.ExcelModernFile
            };

            // Return for download
            return reportDto;
        }

        List<AttachmentResponse> fileList = new()
        {
            new(
                FileContentTypes.ExcelModernFile,
                $"Mandatory Training Report - {data.Name}.xlsx",
                fileData.ToArray())
        };

        List<string> recordIds = data.Modules
            .Where(record => record.RecordId is not null)
            .Select(record => record.RecordId.ToString())
            .ToList();

        foreach (string recordId in recordIds)
        {
            Result<AttachmentResponse> attempt = await _attachmentService.GetAttachmentFile(AttachmentType.TrainingCertificate, recordId, cancellationToken);

            if (attempt.IsFailure)
                continue;

            fileList.Add(attempt.Value);
        }

        // Create ZIP file
        using MemoryStream memoryStream = new();
        using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create))
        {
            foreach (AttachmentResponse file in fileList)
            {
                ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(file.FileName);
                await using StreamWriter streamWriter = new(zipArchiveEntry.Open());
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
