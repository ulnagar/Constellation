using Constellation.Core.Enums;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IStaffRepository
    {
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
}