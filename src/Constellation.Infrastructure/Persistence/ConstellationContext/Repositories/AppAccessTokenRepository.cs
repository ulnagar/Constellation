using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class AppAccessTokenRepository : IAppAccessTokenRepository
    {
        private readonly AppDbContext _context;

        public AppAccessTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public AppAccessToken WithDetails(string id)
        {
            return _context.AspNetAccessTokens.SingleOrDefault(a => a.AccessToken.ToString() == id);
        }

        public AppAccessToken WithFilter(Expression<Func<AppAccessToken, bool>> predicate)
        {
            return _context.AspNetAccessTokens
                .FirstOrDefault(predicate);
        }

        public ICollection<AppAccessToken> All()
        {
            return _context.AspNetAccessTokens
                .ToList();
        }

        public ICollection<AppAccessToken> AllWithFilter(Expression<Func<AppAccessToken, bool>> predicate)
        {
            return _context.AspNetAccessTokens
                .Where(predicate)
                .ToList();
        }

        public void Add(AppAccessToken entity)
        {
            _context.Set<AppAccessToken>().Add(entity);
        }

        public void AddAll(IEnumerable<AppAccessToken> entities)
        {
            _context.Set<AppAccessToken>().AddRange(entities);
        }
    }
}