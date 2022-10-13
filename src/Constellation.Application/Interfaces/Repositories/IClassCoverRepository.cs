using Constellation.Application.Interfaces.Providers;
using Constellation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IClassCoverRepository<T> where T : ClassCover
    {
        T WithDetails(int id);
        T WithFilter(Expression<Func<T, bool>> predicate);
        T GetForExistCheck(int id);
        ICollection<T> All();
        ICollection<T> AllWithFilter(Expression<Func<T, bool>> predicate);
        ICollection<T> AllUpcoming();
        ICollection<T> AllOutdated();
        Task<T> ForEditAsync(int id);
        Task<ICollection<T>> ForClassworkNotifications(DateTime absenceDate, int offeringId);
    }
}