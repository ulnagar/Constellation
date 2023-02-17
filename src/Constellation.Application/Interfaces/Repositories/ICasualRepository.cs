using Constellation.Core.Models.Covers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface ICasualRepository
    {
        Task<Casual?> GetById(int id, CancellationToken cancellationToken = default);
        Task<List<Casual>> GetAllActive(CancellationToken cancellationToken = default);
        Casual WithDetails(int id);
        Casual WithFilter(Expression<Func<Casual, bool>> predicate);
        Task<Casual> GetForExistCheck(int id);
        Task<Casual> WithFilterAsync(Expression<Func<Casual, bool>> predicate);
        ICollection<Casual> All();
        ICollection<Casual> AllWithFilter(Expression<Func<Casual, bool>> predicate);
        ICollection<Casual> AllActive();
        Task<ICollection<Casual>> AllActiveAsync();
        ICollection<Casual> AllWithoutAdobeConnectDetails();
        Task<ICollection<Casual>> ForListAsync(Expression<Func<Casual, bool>> predicate);
        Task<Casual> ForDetailsDisplayAsync(int id);
        Task<Casual> ForEditAsync(int id);
        Task<ICollection<Casual>> ForSelectionAsync();
        Task<Casual> ForExistCheckFromLogin(string login);
        Task<bool> AnyWithId(int id);
        Task<bool> AnyWithPortalUsername(string portalUsername);
    }
}