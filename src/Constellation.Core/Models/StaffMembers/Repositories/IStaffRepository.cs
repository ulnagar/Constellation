namespace Constellation.Core.Models.StaffMembers.Repositories;

using Constellation.Core.Models.Faculties.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.ValueObjects;
using Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tutorials.Identifiers;
using ValueObjects;

public interface IStaffRepository
{
    Task<List<StaffMember>> GetAll(CancellationToken cancellationToken = default);
    Task<StaffMember?> GetById(StaffId staffId, CancellationToken cancellationToken = default);
    Task<StaffMember?> GetByEmployeeId(EmployeeId employeeId, CancellationToken cancellationToken = default);
    Task<StaffMember?> GetByEdvalCode(string edvalCode, CancellationToken cancellationToken = default);
    Task<List<StaffMember>> GetListFromIds(List<StaffId> staffIds, CancellationToken cancellationToken = default);
    Task<List<StaffMember>> GetCurrentTeachersForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<StaffMember>> GetCurrentTeachersForTutorial(TutorialId tutorialId, CancellationToken cancellationToken = default);
    Task<List<StaffMember>> GetPrimaryTeachersForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<StaffMember>> GetFacultyHeadTeachers(FacultyId facultyId, CancellationToken cancellationToken = default);
    Task<List<StaffMember>> GetFacultyHeadTeachersForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<StaffMember>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<StaffId>> GetAllActiveStaffIds(CancellationToken cancellationToken = default);
    Task<List<StaffMember>> GetActiveFromSchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<int> GetCountCurrentStaffFromSchool(string schoolCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current staff member with the specified email address.
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StaffMember?> GetCurrentByEmailAddress(EmailAddress emailAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get any staff member with the specified email address. Can include a deleted staff member.
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StaffMember?> GetAnyByEmailAddress(EmailAddress emailAddress, CancellationToken cancellationToken = default);
    Task<StaffMember> GetFromName(string name, CancellationToken cancellationToken = default);

    void Insert(StaffMember member);
}