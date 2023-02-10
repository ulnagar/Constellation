namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface IStaffRepository
{
    Task<Staff?> GetById(string staffId, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetListFromIds(List<string> staffIds, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetCurrentTeachersForOffering(int offeringId, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetFacultyHeadTeachers(Guid facultyId, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetFacultyHeadTeachersForOffering(int offeringId, CancellationToken cancellationToken = default);
    Task<List<Staff>> GetAllActive(CancellationToken cancellationToken = default);
    Staff WithDetails(string id);
    Staff WithFilter(Expression<Func<Staff, bool>> predicate);
    Task<Staff> GetForExistCheck(string id);
    Task<Staff> WithFilterAsync(Expression<Func<Staff, bool>> predicate);
    ICollection<Staff> All();
    ICollection<Staff> AllWithFilter(Expression<Func<Staff, bool>> predicate);
    ICollection<Staff> AllActive();
    Task<ICollection<Staff>> AllActiveAsync();
    ICollection<Staff> AllInactive();
    ICollection<Staff> AllFromSchool(string code);
    ICollection<Staff> AllActiveFromSchool(string code);
    ICollection<Staff> AllFromFaculty(Guid facultyId);
    ICollection<Staff> AllWithoutAdobeConnectInfo();
    ICollection<Staff> AllWithActiveClasses();
    Task<Staff> FromEmailForExistCheck(string email);
    Task<Staff> FromIdForExistCheck(string id);

    Task<ICollection<Staff>> ForListAsync(Expression<Func<Staff, bool>> predicate);
    Task<Staff> ForDetailDisplayAsync(string id);
    Task<ICollection<Staff>> ForSelectionAsync();
    Task<Staff> ForEditAsync(string id);
    Task<bool> AnyWithId(string id);
    Task<Staff> ForDeletion(string id);
    Task<Staff> GetFromName(string name);
}