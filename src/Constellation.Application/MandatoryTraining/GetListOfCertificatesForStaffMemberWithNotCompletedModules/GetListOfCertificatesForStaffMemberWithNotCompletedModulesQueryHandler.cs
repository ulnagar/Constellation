namespace Constellation.Application.MandatoryTraining.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Models;
using Constellation.Core.Models.Faculty;
using Constellation.Core.Models.Faculty.Repositories;
using Constellation.Core.Models.Training.Contexts.Modules;
using Constellation.Core.Shared;
using Core.Models.Faculty.ValueObjects;
using Core.Models.Training.Repositories;
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

    public GetListOfCertificatesForStaffMemberWithNotCompletedModulesQueryHandler(
        ITrainingModuleRepository trainingRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IFacultyRepository facultyRepository)
    {
        _trainingRepository = trainingRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<StaffCompletionListDto>> Handle(GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery request, CancellationToken cancellationToken)
    {
        StaffCompletionListDto data = new();

        // - Get all modules
        List<TrainingModule> modules = await _trainingRepository.GetAllModules(cancellationToken);

        // - Get staff member
        Staff staff = await _staffRepository.GetById(request.StaffId, cancellationToken);

        // - Get Staff school details
        School school = await _schoolRepository.GetById(staff.SchoolCode, cancellationToken);

        // - Get Staff Faculties
        List<Faculty> faculties = await _facultyRepository.GetCurrentForStaffMember(staff.StaffId, cancellationToken);
     
        data.StaffId = request.StaffId;
        data.Name = staff.DisplayName;
        data.SchoolName = school.Name;
        data.EmailAddress = staff.EmailAddress;
        data.Faculties = faculties.Select(faculty => faculty.Name).ToList();

        foreach (TrainingModule module in modules)
        {
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
