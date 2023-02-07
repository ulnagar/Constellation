using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface ITimetablePeriodRepository
    {
        Task<List<TimetablePeriod>> GetByDayAndOfferingId(int dayNumber, int offeringId, CancellationToken cancellationToken = default);
        Task<List<TimetablePeriod>> GetAll(CancellationToken cancellationToken = default);
        Task<List<TimetablePeriod>> GetAllFromTimetable(List<string> timetables, CancellationToken cancellationToken = default);
        TimetablePeriod WithDetails(int id);
        TimetablePeriod WithFilter(Expression<Func<TimetablePeriod, bool>> predicate);
        ICollection<TimetablePeriod> All();
        ICollection<TimetablePeriod> AllWithFilter(Expression<Func<TimetablePeriod, bool>> predicate);
        ICollection<TimetablePeriod> AllFromDay(int day);
        ICollection<TimetablePeriod> AllActive();
        ICollection<TimetablePeriod> AllForStudent(string studentId);
        Task<ICollection<TimetablePeriod>> ForSelectionAsync();
        Task<ICollection<TimetablePeriod>> ForGraphicalDisplayAsync();
        Task<TimetablePeriod> ForEditAsync(int id);
    }
}