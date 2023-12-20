namespace Constellation.Application.Training.Modules.GenerateOverallReport;

using Abstractions.Messaging;
using Core.Models;
using Core.Models.Faculty;
using Core.Models.Faculty.Repositories;
using Core.Models.Training.Contexts.Modules;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Identifiers;
using Core.Models.Training.Repositories;
using Core.Shared;
using DTOs;
using GroupTutorials.Events;
using Interfaces.Repositories;
using Interfaces.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GenerateOveralReportCommandHandler
: ICommandHandler<GenerateOveralReportCommand, FileDto>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly ITrainingModuleRepository _moduleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly IExcelService _excelService;
    private readonly ILogger _logger;

    public GenerateOveralReportCommandHandler(
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
        _logger = logger;
    }

    public async Task<Result<FileDto>> Handle(GenerateOveralReportCommand request, CancellationToken cancellationToken)
    {
        List<Staff> staff = await _staffRepository.GetAllActive(cancellationToken);

        List<TrainingRole> roles = await _roleRepository.GetAllRoles(cancellationToken);

        List<TrainingModule> modules = await _moduleRepository.GetAllModules(cancellationToken);

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

                continue;
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
                    .CompletedDate;

                moduleStatuses.Add(new(
                    module.Id,
                    required,
                    completed));
            }

            staffStatuses.Add(new(
                member.StaffId,
                member.GetName(),
                school.Code,
                school.Name,
                memberFaculties.Select(entry => entry.Name).ToArray(),
                moduleStatuses));
        }

        var fileStream = await _excelService.CreateTrainingModuleOverallReportFile(moduleDetails, staffStatuses, cancellationToken);
    }
}