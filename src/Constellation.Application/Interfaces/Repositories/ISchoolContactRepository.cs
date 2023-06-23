namespace Constellation.Application.Interfaces.Repositories;

using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public interface ISchoolContactRepository
{
    Task<List<SchoolContact>> GetPrincipalsForSchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<SchoolContact?> GetWithRolesByEmailAddress(string emailAddress, CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetWithRolesBySchool(string schoolCode, CancellationToken cancellationToken = default);
    Task<List<SchoolContact>> GetBySchoolAndRole(string schoolCode, string selectedRole, CancellationToken cancellationToken = default);

    SchoolContact WithDetails(int id);
    SchoolContact WithFilter(Expression<Func<SchoolContact, bool>> predicate);
    SchoolContact GetForExistCheck(int id);
    ICollection<SchoolContact> All();
    ICollection<SchoolContact> AllWithFilter(Expression<Func<SchoolContact, bool>> predicate);
    ICollection<SchoolContact> AllFromSchool(string schoolCode);
    ICollection<SchoolContact> AllWithRole(string role);
    ICollection<SchoolContact> AllFromSchoolWithRole(string schoolCode, string role);
    ICollection<SchoolContact> AllWithoutRole();
    Task<SchoolContact> FromEmailForExistCheck(string email);
    Task<SchoolContact> FromIdForExistCheck(int id);
    Task<ICollection<SchoolContact>> ScienceTeachersForLessonsPortalAdmin();
    Task<SchoolContact> ForAudit(int id);
    Task<bool> IsContactAtSecondarySchool(int id);
    Task<bool> IsContactAtPrimarySchool(int id);
    Task<ICollection<SchoolContact>> ForSelectionAsync();
    Task<ICollection<SchoolContact>> AllWithoutActiveRoleAsync();
    Task<ICollection<SchoolContact>> AllWithActiveRoleAsync();
    Task<ICollection<SchoolContact>> AllWithStudentsInGradeAsync(Grade grade);
    Task<ICollection<SchoolContact>> AllWithRoleAsync(string role);
    Task<SchoolContact> ForEditAsync(int id);
    Task<ICollection<string>> EmailAddressesOfAllInRoleAtSchool(string studentId, string role);
    Task<bool> AnyWithId(int id);
    Task<SchoolContact> ForEditFromEmail(string email);
    Task<ICollection<SchoolContact>> ForBulkUpdate();
}