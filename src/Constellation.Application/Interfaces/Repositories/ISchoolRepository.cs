using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface ISchoolRepository
    {
        School WithDetails(string code);
        School WithFilter(Expression<Func<School, bool>> predicate);
        ICollection<School> All();
        ICollection<School> AllWithFilter(Expression<Func<School, bool>> predicate);
        ICollection<School> AllWithStudents();
        ICollection<School> AllWithStaff();
        ICollection<School> AllWithEither();
        ICollection<School> AllWithBoth();
        ICollection<School> AllWithNeither();
        Task<IDictionary<string, string>> AllForLessonsPortal();
        Task<ICollection<School>> AllWithStudentsForAbsenceSettingsAsync();
        Task<ICollection<School>> ForSelectionAsync();
        Task<ICollection<School>> ForListAsync(Expression<Func<School, bool>> predicate);
        Task<School> ForEditAsync(string id);
        Task<School> ForDetailDisplayAsync(string id);
        Task<bool> IsPartnerSchoolWithStudents(string code);
        Task<bool> AnyWithId(string id);
        Task<ICollection<School>> ForBulkUpdate();
        IList<MapLayer> GetForMapping(IList<string> schoolCodes);
        Task<ICollection<School>> ForTrackItSync();
        Task<ICollection<string>> AHPSchoolCodes();
    }
}