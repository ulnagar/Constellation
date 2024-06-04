namespace Constellation.Application.Training.Modules.GenerateOverallReport;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training.Contexts.Modules;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Errors;
using Core.Models.Training.Identifiers;
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
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public GenerateOverviewReportCommandHandler(
        ITrainingRoleRepository roleRepository,
        ITrainingModuleRepository moduleRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IFacultyRepository facultyRepository,
        IExcelService excelService,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _moduleRepository = moduleRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _facultyRepository = facultyRepository;
        _excelService = excelService;
        _logger = logger.ForContext<GenerateOverviewReportCommand>();
    }

    public async Task<Result<FileDto>> Handle(GenerateOverviewReportCommand request, CancellationToken cancellationToken)
    {
        List<Staff> staff = await _staffRepository.GetAllActive(cancellationToken);

        if (!staff.Any())
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NoneFound, true)
                .Warning("Could not generate Overview Report");

            return Result.Failure<FileDto>(DomainErrors.Partners.Staff.NoneFound);
        }

        List<TrainingRole> roles = await _roleRepository.GetAllRoles(cancellationToken);

        if (!roles.Any())
        {
            _logger
                .ForContext(nameof(Error), TrainingErrors.Role.NoneFound, true)
                .Warning("Could not generate Overview Report");

            return Result.Failure<FileDto>(TrainingErrors.Role.NoneFound);
        }

        List<TrainingModule> modules = await _moduleRepository.GetAllModules(cancellationToken);

        if (!roles.Any())
        {
            _logger
                .ForContext(nameof(Error), TrainingErrors.Module.NoneFound, true)
                .Warning("Could not generate Overview Report");

            return Result.Failure<FileDto>(TrainingErrors.Module.NoneFound);
        }

        List<School> schools = await _schoolRepository.GetAllActive(cancellationToken);

        List<Faculty> faculties = await _facultyRepository.GetAll(cancellationToken);

        List<StaffStatus> staffStatuses = new();

        List<ModuleDetails> moduleDetails = new();

        foreach (TrainingModule module in modules)
        {
            moduleDetails.Add(new(
                module.Id,
                module.Name,
                module.Expiry));
        }

        foreach (Staff member in staff)
        {
            School school = schools.FirstOrDefault(entry => entry.Code == member.SchoolCode);

            if (school is null)
            {
                _logger
                    .ForContext(nameof(Staff), member, true)
                    .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(member.SchoolCode), true)
                    .Warning("Could not include staff member in report");
            }

            List<Faculty> memberFaculties = faculties
                .Where(faculty =>
                    faculty.Members.Any(entry => 
                        entry.StaffId == member.StaffId && 
                        !entry.IsDeleted))
                .ToList();

            List<TrainingModuleId> requiredModuleIds = roles
                .Where(role =>
                    role.Members.Any(entry =>
                        entry.StaffId == member.StaffId))
                .SelectMany(role => role.Modules)
                .Select(module => module.ModuleId)
                .ToList();

            List<ModuleStatus> moduleStatuses = new();

            foreach (TrainingModule module in modules)
            {
                bool required = requiredModuleIds.Contains(module.Id);

                DateOnly? completed = module
                    .Completions
                    .Where(entry => entry.StaffId == member.StaffId)
                    .MaxBy(entry => entry.CompletedDate)
                    ?.CompletedDate;

                moduleStatuses.Add(new(
                    module.Id,
                    required,
                    completed));
            }

            staffStatuses.Add(new(
                member.StaffId,
                member.GetName(),
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