namespace Constellation.Application.Domains.Training.Queries.GenerateOverallReport;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Errors;
using Core.Models.Training.Repositories;
using Core.Shared;
using DTOs;
using Helpers;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GenerateOverviewReportCommandHandler
: ICommandHandler<GenerateOverviewReportCommand, FileDto>
{
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public GenerateOverviewReportCommandHandler(
        ITrainingModuleRepository moduleRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IFacultyRepository facultyRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _moduleRepository = moduleRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _facultyRepository = facultyRepository;
        _excelService = excelService;
        _logger = logger.ForContext<GenerateOverviewReportCommand>();
    }

    public async Task<Result<FileDto>> Handle(GenerateOverviewReportCommand request, CancellationToken cancellationToken)
    {
        List<StaffMember> staff = await _staffRepository.GetAllActive(cancellationToken);

        if (!staff.Any())
        {
            _logger
                .ForContext(nameof(Error), StaffMemberErrors.NoneFound, true)
                .Warning("Could not generate Overview Report");

            return Result.Failure<FileDto>(StaffMemberErrors.NoneFound);
        }

        List<TrainingModule> modules = await _moduleRepository.GetAllModules(cancellationToken);

        if (modules.Count == 0)
        {
            _logger
                .ForContext(nameof(Error), TrainingModuleErrors.NoneFound, true)
                .Warning("Could not generate Overview Report");

            return Result.Failure<FileDto>(TrainingModuleErrors.NoneFound);
        }

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);

        List<StaffStatus> staffStatuses = new();

        List<ModuleDetails> moduleDetails = new();

        foreach (TrainingModule module in modules)
        {
            if (module.IsDeleted) continue;

            moduleDetails.Add(new(
                module.Id,
                module.Name,
                module.Expiry));
        }

        foreach (StaffMember member in staff)
        {
            School school = member.CurrentAssignment is not null
                ? schools.FirstOrDefault(entry => entry.Code == member.CurrentAssignment.SchoolCode)
                : null;

            if (school is null)
            {
                _logger
                    .ForContext(nameof(StaffMember), member, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(member.CurrentAssignment?.SchoolCode), true)
                    .Warning("Could not include staff member in report");
            }

            List<Faculty> memberFaculties = faculties
                .Where(faculty =>
                    faculty.Members.Any(entry =>
                        entry.StaffId == member.Id &&
                        !entry.IsDeleted))
                .ToList();

            List<ModuleStatus> moduleStatuses = new();

            foreach (TrainingModule module in modules)
            {
                if (module.IsDeleted) continue;

                bool required = module.Assignees.Any(entry => entry.StaffId == member.Id);

                DateOnly? completed = module
                    .Completions
                    .Where(entry => entry.StaffId == member.Id)
                    .MaxBy(entry => entry.CompletedDate)
                    ?.CompletedDate;

                moduleStatuses.Add(new(
                    module.Id,
                    required,
                    completed));
            }

            staffStatuses.Add(new(
                member.Id,
                member.Name,
                school?.Code ?? string.Empty,
                school?.Name ?? string.Empty,
                memberFaculties.Select(entry => entry.Name).ToArray(),
                moduleStatuses));
        }

        MemoryStream fileStream = await _excelService.CreateTrainingModuleOverallReportFile(moduleDetails, staffStatuses, cancellationToken);

        return new FileDto()
        {
            FileData = fileStream.ToArray(),
            FileName = "Mandatory Training Overview Report.xlsx",
            FileType = FileContentTypes.ExcelModernFile
        };
    }
}