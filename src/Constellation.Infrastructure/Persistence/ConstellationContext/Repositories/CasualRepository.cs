using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class CasualRepository : ICasualRepository
    {
        private readonly AppDbContext _context;

        public CasualRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<Casual> Collection()
        {
            return _context.Casuals
                .Include(c => c.AdobeConnectOperations)
                    .ThenInclude(operation => operation.Room)
                .Include(c => c.School)
                .Include(c => c.ClassCovers)
                    .ThenInclude(cover => cover.Offering)
                .OrderBy(c => c.LastName);
        }

        public async Task<Casual?> GetById(
            int id, 
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<Casual>()
                .FirstOrDefaultAsync(casual => casual.Id == id, cancellationToken);

        public async Task<bool> AnyWithId(int id)
        {
            return await _context.Casuals
                .AnyAsync(casual => casual.Id == id);
        }

        public async Task<bool> AnyWithPortalUsername(string portalUsername)
        {
            return await _context.Casuals
                .AnyAsync(casual => casual.PortalUsername == portalUsername);
        }

        public Casual WithDetails(int id)
        {
            return Collection()
                .SingleOrDefault(c => c.Id == id);
        }

        public Casual WithFilter(Expression<Func<Casual, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public async Task<Casual> GetForExistCheck(int id)
        {
            return await _context.Casuals
                .SingleOrDefaultAsync(casual => casual.Id == id);
        }

        public async Task<Casual> ForExistCheckFromLogin(string login)
        {
            return await _context.Casuals
                .SingleOrDefaultAsync(casual => casual.PortalUsername == login);
        }

        public async Task<Casual> WithFilterAsync(Expression<Func<Casual, bool>> predicate)
        {
            return await Collection()
                .FirstOrDefaultAsync(predicate);
        }

        public ICollection<Casual> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<Casual> AllWithFilter(Expression<Func<Casual, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<Casual> AllActive()
        {
            return Collection()
                .Where(c => c.IsDeleted == false)
                .ToList();
        }

        public async Task<ICollection<Casual>> AllActiveAsync()
        {
            return await _context.Casuals
                .Where(c => c.IsDeleted == false)
                .ToListAsync();
        }

        public ICollection<Casual> AllWithoutAdobeConnectDetails()
        {
            return Collection()
                .Where(c => c.AdobeConnectPrincipalId == null || c.AdobeConnectPrincipalId.Trim() == string.Empty)
                .ToList();
        }

        public async Task<ICollection<Casual>> ForListAsync(Expression<Func<Casual, bool>> predicate)
        {
            return await _context.Casuals
                .Include(casual => casual.School)
                .Include(casual => casual.ClassCovers)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Casual> ForDetailsDisplayAsync(int id)
        {
            return await _context.Casuals
                .Include(casual => casual.School)
                .Include(casual => casual.ClassCovers)
                .ThenInclude(cover => cover.Offering)
                .FirstOrDefaultAsync(casual => casual.Id == id);
        }

        public async Task<Casual> ForEditAsync(int id)
        {
            return await _context.Casuals
                .SingleOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ICollection<Casual>> ForSelectionAsync()
        {
            return await _context.Casuals
                .Where(casual => !casual.IsDeleted)
                .ToListAsync();
        }
    }
}