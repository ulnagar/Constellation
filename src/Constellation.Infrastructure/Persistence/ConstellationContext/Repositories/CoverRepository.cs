using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class CoverRepository : ICoverRepository
    {
        private readonly AppDbContext _context;

        public CoverRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ICollection<ClassCover>> ForListAsync(Expression<Func<ClassCover, bool>> predicate)
        {
            return await _context.Covers
                .Include(cover => ((CasualClassCover)cover).Casual)
                .ThenInclude(casual => casual.School)
                .Include(cover => ((TeacherClassCover)cover).Staff)
                .ThenInclude(teacher => teacher.School)
                .Include(cover => cover.Offering)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<ClassCover> ForDetailsDisplayAsync(int id)
        {
            return await _context.Covers
                .Include(cover => ((CasualClassCover)cover).Casual)
                .ThenInclude(casual => casual.School)
                .Include(cover => ((TeacherClassCover)cover).Staff)
                .ThenInclude(teacher => teacher.School)
                .Include(cover => cover.Offering)
                .SingleOrDefaultAsync(cover => cover.Id == id);
        }

        public async Task<string> CoverTypeForCancellationAsync(int id)
        {
            var cover = await _context.Covers.SingleOrDefaultAsync(cover => cover.Id == id);

            return cover switch
            {
                CasualClassCover c1 => "Casual",
                TeacherClassCover t1 => "Teacher",
                _ => "Unknown",
            };
        }

        public async Task<ICollection<ClassCover>> ForOperationCancellation()
        {
            var yesterday = DateTime.Today.AddDays(-1);

            return await _context.Covers
                .Include(cover => cover.AdobeConnectOperations)
                .Where(cover => cover.EndDate < yesterday || cover.IsDeleted)
                .ToListAsync();
        }

        public async Task<ClassCover> GetForExistCheck(int id)
        {
            return await _context.Covers
                .SingleOrDefaultAsync(cover => cover.Id == id);
        }

        public async Task<ICollection<ClassCover>> OutstandingForCasual(int casualId)
        {
            return await _context.Covers
                .Where(cover => !cover.IsDeleted && (cover.IsCurrent || cover.IsFuture) && ((CasualClassCover)cover).CasualId == casualId)
                .ToListAsync();
        }

        public async Task<ClassCover> ForUpdate(int id)
        {
            return await _context.Covers
                .Include(cover => cover.AdobeConnectOperations)
                .Include(cover => cover.MSTeamOperations)
                .SingleOrDefaultAsync(cover => cover.Id == id);
        }
    }
}
