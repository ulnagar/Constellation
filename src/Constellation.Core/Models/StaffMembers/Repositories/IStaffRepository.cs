using Constellation.Core.Models.Faculties.Identifiers;

namespace Constellation.Core.Models.StaffMembers.Repositories;

using Constellation.Core.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface IStaffRepository
{
    Task<List<Staff>> GetAll(CancellationToken cancellationToken = default);
    Task<Staff?> GetById(string staffId, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetListFromIds(List<string> staffIds, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetCurrentTeachersForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetPrimaryTeachersForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetFacultyHeadTeachers(FacultyId facultyId, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetFacultyHeadTeachersForOffering(OfferingId offeringId, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetAllActive(CancellationToken cancellationToken = default);
    Task<List<string>> GetAllActiveStaffIds(CancellationToken cancellationToken = default);
    Task<List<Staff>> GetActiveFromSchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<int> GetCountCurrentStaffFromSchool(string schoolCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current staff member with the specified email address.
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Staff?> GetCurrentByEmailAddress(string emailAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get any staff member with the specified email address. Can include a deleted staff member.
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Staff?> GetAnyByEmailAddress(string emailAddress, CancellationToken cancellationToken = default);
    Staff WithDetails(string id);
    Task<Staff> GetForExistCheck(string id);
   
    ICollection<Staff> AllWithoutAdobeConnectInfo();
    Task<Staff> FromEmailForExistCheck(string email);
    Task<Staff> FromIdForExistCheck(string id);

    Task<ICollection<Staff>> ForListAsync(Expression<Func<Staff, bool>> predicate);
    Task<Staff> ForDetailDisplayAsync(string id);
    Task<Staff> ForEditAsync(string id);
    Task<bool> AnyWithId(string id);
    Task<Staff> ForDeletion(string id);
    Task<Staff> GetFromName(string name);

    void Insert(Staff member);
}