using Constellation.Application.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Constellation.Application.Interfaces.Repositories
{
    public interface IAppAccessTokenRepository
    {
        AppAccessToken WithDetails(string id);
        AppAccessToken WithFilter(Expression<Func<AppAccessToken, bool>> predicate);
        ICollection<AppAccessToken> All();
        ICollection<AppAccessToken> AllWithFilter(Expression<Func<AppAccessToken, bool>> predicate);
        void Add(AppAccessToken entity);
        void AddAll(IEnumerable<AppAccessToken> entities);
    }
}