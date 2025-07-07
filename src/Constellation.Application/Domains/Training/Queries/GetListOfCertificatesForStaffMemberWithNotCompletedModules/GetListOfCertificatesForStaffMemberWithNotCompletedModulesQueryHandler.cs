namespace Constellation.Application.Domains.Training.Queries.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Abstractions.Messaging;
using Core.Errors;
using Core.Models;
using Core.Models.Faculties;
using Core.Models.Faculties.Repositories;
using Core.Models.Faculties.ValueObjects;
using Core.Models.StaffMembers;
using Core.Models.StaffMembers.Errors;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.StaffMembers.Repositories;
using Core.Models.Training;
using Core.Models.Training.Repositories;
using Core.Shared;
using Interfaces.Repositories;
using Models;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetListOfCertificatesForStaffMemberWithNotCompletedModulesQueryHandler
    : IQueryHandler<GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery, StaffCompletionListDto>
{
    private readonly ITrainingModuleRepository _trainingRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IFacultyRepository _facultyRepository;
    private readonly ILogger _logger;

    public GetListOfCertificatesForStaffMemberWithNotCompletedModulesQueryHandler(
        ITrainingModuleRepository trainingRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IFacultyRepository facultyRepository,
        ILogger logger)
    {
        _trainingRepository = trainingRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _facultyRepository = facultyRepository;
        _logger = logger.ForContext<GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery>();
    }

    public async Task<Result<StaffCompletionListDto>> Handle(GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery request, CancellationToken cancellationToken)
    {
        StaffCompletionListDto data = new();

        StaffMember staff = await _staffRepository.GetById(request.StaffId, cancellationToken);

        if (staff is null)
        {
            _logger
                .ForContext(nameof(GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery), request, true)
                .ForContext(nameof(Error), StaffMemberErrors.NotFound(request.StaffId), true)
                .Warning("Failed to retrieve list of Training Completions for staff member");

            return Result.Failure<StaffCompletionListDto>(StaffMemberErrors.NotFound(request.StaffId));
        }

        School school = staff.CurrentAssignment is not null
            ? await _schoolRepository.GetById(staff.CurrentAssignment.SchoolCode, cancellationToken)
            : null;

        if (school is null)
        {
            _logger
                .ForContext(nameof(GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.Partners.School.NotFound(staff.CurrentAssignment?.SchoolCode), true)
                .Warning("Failed to retrieve list of Training Completions for staff member");

            return Result.Failure<StaffCompletionListDto>(DomainErrors.Partners.School.NotFound(staff.CurrentAssignment?.SchoolCode));
        }

        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(staff.Id, cancellationToken);

        data.StaffId = request.StaffId;
        data.Name = staff.Name.DisplayName;
        data.SchoolName = school!.Name;
        data.EmailAddress = staff.EmailAddress.Email;
        data.Faculties = faculties.Select(faculty => faculty.Name).ToList();

        List<TrainingModule> modules = await _trainingRepository.GetAllModules(cancellationToken);

        foreach (TrainingModule module in modules)
        {
            bool required = module.Assignees.Any(entry => entry.StaffId == request.StaffId);

            if (module.IsDeleted)
                continue;

            TrainingCompletion record = module.Completions
                .Where(record =>
                    record.StaffId == staff.Id &&
                    !record.IsDeleted)
                .MaxBy(record => record.CompletedDate);

            CompletionRecordExtendedDetailsDto entry = new();
            entry.AddModuleDetails(module);
            entry.AddStaffDetails(staff);

            foreach (Faculty faculty in faculties)
            {
                List<StaffId> headTeacherIds = faculty
                    .Members
                    .Where(member =>
                        !member.IsDeleted &&
                        member.Role == FacultyMembershipRole.Manager)
                    .Select(member => member.StaffId)
                    .ToList();

                List<StaffMember> headTeachers = await _staffRepository
                    .GetListFromIds(headTeacherIds, cancellationToken);

                foreach (StaffMember headTeacher in headTeachers)
                    entry.AddHeadTeacherDetails(faculty, headTeacher);
            }

            if (record is not null)
                entry.AddRecordDetails(record);

            entry.CalculateExpiry();

            if (required || record is not null)
                data.Modules.Add(entry);
        }

        data.Modules = data.Modules.OrderBy(entry => entry.DueDate).ToList();

        return data;
    }
}
