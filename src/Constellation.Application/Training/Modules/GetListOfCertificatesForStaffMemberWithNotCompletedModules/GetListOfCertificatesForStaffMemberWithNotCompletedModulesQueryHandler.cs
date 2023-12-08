namespace Constellation.Application.Training.Modules.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Shared;
using Core.Errors;
using Core.Models.Faculty.ValueObjects;
using Core.Models.Training.Contexts.Roles;
using Core.Models.Training.Identifiers;
using Core.Models.Training.Repositories;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetListOfCertificatesForStaffMemberWithNotCompletedModulesQueryHandler
    : IQueryHandler<GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery, StaffCompletionListDto>
{
    private readonly ITrainingRoleRepository _roleRepository;
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger _logger;

    public GetListOfCertificatesForStaffMemberWithNotCompletedModulesQueryHandler(
        ITrainingRoleRepository roleRepository,
        ITrainingModuleRepository trainingRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IFacultyRepository facultyRepository,
        ILogger logger)
    {
        _roleRepository = roleRepository;
        _trainingRepository = trainingRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _facultyRepository = facultyRepository;
        _logger = logger.ForContext<GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery>();
    }

    public async Task<Result<StaffCompletionListDto>> Handle(GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery request, CancellationToken cancellationToken)
    {
        StaffCompletionListDto data = new();

        Staff staff = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staff is null)
        {
            _logger
                .ForContext(nameof(GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.Staff.NotFound(request.StaffId), true)
                .Warning("Failed to retrieve list of Training Completions for staff member");

            return Result.Failure<StaffCompletionListDto>(DomainErrors.Partners.Staff.NotFound(request.StaffId));
        }

        School school = await _schoolRepository.GetById(staff.SchoolCode, cancellationToken);

        if (school is null)
        {
            _logger
                .ForContext(nameof(GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(staff.SchoolCode), true)
                .Warning("Failed to retrieve list of Training Completions for staff member");

            return Result.Failure<StaffCompletionListDto>(DomainErrors.Partners.School.NotFound(staff.SchoolCode));
        }

        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(staff.StaffId, cancellationToken);

        data.StaffId = request.StaffId;
        data.Name = staff.DisplayName;
        data.SchoolName = school!.Name;
        data.EmailAddress = staff.EmailAddress;
        data.Faculties = faculties.Select(faculty => faculty.Name).ToList();

        List<TrainingRole> roles = await _roleRepository.GetRolesForStaffMember(request.StaffId, cancellationToken);

        List<TrainingModuleId> moduleIds = roles
            .SelectMany(role => role.Modules)
            .Select(module => module.ModuleId)
            .Distinct()
            .ToList();
        
        foreach (TrainingModuleId moduleId in moduleIds)
        {
            TrainingModule module = await _trainingRepository.GetModuleById(moduleId, cancellationToken);

            if (module.IsDeleted)
                continue;

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

            if (record is not null)
                entry.AddRecordDetails(record);

            entry.CalculateExpiry();

            data.Modules.Add(entry);
        }

        return data;
    }
}
