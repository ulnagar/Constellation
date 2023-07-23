namespace Constellation.Application.MandatoryTraining.GetListOfCertificatesForStaffMemberWithNotCompletedModules;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.MandatoryTraining.Models;
using Constellation.Core.Abstractions;
using Constellation.Core.Models;
using Constellation.Core.Models.MandatoryTraining;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetListOfCertificatesForStaffMemberWithNotCompletedModulesQueryHandler 
    : IQueryHandler<GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery, StaffCompletionListDto>
{
    private readonly ITrainingModuleRepository _trainingModuleRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly ISchoolRepository _schoolRepository;
    private readonly IFacultyRepository _facultyRepository;

    public GetListOfCertificatesForStaffMemberWithNotCompletedModulesQueryHandler(
        ITrainingModuleRepository trainingModuleRepository,
        IStaffRepository staffRepository,
        ISchoolRepository schoolRepository,
        IFacultyRepository facultyRepository)
    {
        _trainingModuleRepository = trainingModuleRepository;
        _staffRepository = staffRepository;
        _schoolRepository = schoolRepository;
        _facultyRepository = facultyRepository;
    }

    public async Task<Result<StaffCompletionListDto>> Handle(GetListOfCertificatesForStaffMemberWithNotCompletedModulesQuery request, CancellationToken cancellationToken)
    {
        StaffCompletionListDto data = new();

        // - Get all modules
        List<TrainingModule> modules = await _trainingModuleRepository.GetAllCurrent(cancellationToken);

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

            if (record is not null)
                entry.AddRecordDetails(record);

            entry.CalculateExpiry();

            data.Modules.Add(entry);
        }

        return data;
    }
}
