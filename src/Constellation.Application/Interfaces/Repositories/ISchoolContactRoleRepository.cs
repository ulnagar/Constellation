using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface ISchoolContactRoleRepository
    {
        Task<bool> Exists(int ContactId, string SchoolCode, string Position, CancellationToken cancellationToken = default);
        Task<List<SchoolContactRole>> GetForContact(int ContactId, CancellationToken cancellationToken = default);

        Task<SchoolContactRole> WithDetails(int id);
        SchoolContactRole WithFilter(Expression<Func<SchoolContactRole, bool>> predicate);
        ICollection<SchoolContactRole> All();
        ICollection<SchoolContactRole> AllWithFilter(Expression<Func<SchoolContactRole, bool>> predicate);
        ICollection<SchoolContactRole> AllFromSchool(string schoolCode);
        ICollection<SchoolContactRole> AllWithRole(string role);
        ICollection<SchoolContactRole> AllFromSchoolWithRole(string schoolCode, string role);
        ICollection<string> AllRoles();
        Task<ICollection<SchoolContactRole>> FromCoordinatorForLessonsPortal(int contactId);
        Task<ICollection<SchoolContactRole>> FromCoordinatorForAbsencesPortal(int contactId);
        Task<ICollection<SchoolContactRole>> AllCurrent();
        Task<ICollection<SchoolContactRole>> AllCurrentWithRole(string selectedRole);
        Task<ICollection<SchoolContactRole>> AllCurrentFromGrade(Grade grade);
        Task<ICollection<string>> ListOfRolesForSelectionAsync();
        Task<bool> AnyWithId(int id);
        Task<SchoolContactRole> ForEdit(int id);
        Task<ICollection<string>> EmailsFromSchoolWithRole(string schoolCode, string role);
        Task<ICollection<SchoolContactRole>> FromSchoolForLessonsNotifications(string schoolCode);
    }
}