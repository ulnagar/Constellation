using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories
{
    public class AdobeConnectRoomRepository : IAdobeConnectRoomRepository
    {
        private readonly AppDbContext _context;

        public AdobeConnectRoomRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<AdobeConnectRoom> Collection()
        {
            return _context.Rooms
                .Include(r => r.OfferingSessions)
                    .ThenInclude(session => session.Offering)
                .Include(r => r.OfferingSessions)
                    .ThenInclude(session => session.Period)
                .Include(r => r.OfferingSessions)
                    .ThenInclude(session => session.Teacher)
                .OrderBy(r => r.Name);
        }

        public async Task<List<AdobeConnectRoom>> GetByOfferingId(
            int offeringId,
            CancellationToken cancellationToken = default) =>
            await _context
                .Set<AdobeConnectRoom>()
                .Where(room => 
                    room.OfferingSessions.Any(session => 
                        session.OfferingId == offeringId && 
                        !session.IsDeleted))
                .Distinct()
                .ToListAsync(cancellationToken);

        public AdobeConnectRoom WithDetails(string id)
        {
            return Collection()
                .SingleOrDefault(d => d.ScoId == id);
        }

        public AdobeConnectRoom WithFilter(Expression<Func<AdobeConnectRoom, bool>> predicate)
        {
            return Collection()
                .FirstOrDefault(predicate);
        }

        public async Task<AdobeConnectRoom> GetForExistCheck(string id)
        {
            return await _context.Rooms
                .SingleOrDefaultAsync(room => room.ScoId == id);
        }

        public ICollection<AdobeConnectRoom> All()
        {
            return Collection()
                .ToList();
        }

        public ICollection<AdobeConnectRoom> AllWithFilter(Expression<Func<AdobeConnectRoom, bool>> predicate)
        {
            return Collection()
                .Where(predicate)
                .ToList();
        }

        public ICollection<AdobeConnectRoom> AllFromSession(OfferingSession session)
        {
            return Collection()
                .Where(r => r.OfferingSessions.Contains(session))
                .ToList();
        }

        public ICollection<AdobeConnectRoom> AllActive()
        {
            return Collection()
                .Where(r => r.IsDeleted == false)
                .ToList();
        }

        public ICollection<AdobeConnectRoom> AllWithSession()
        {
            return Collection()
                .Where(r => r.OfferingSessions.Any(o => o.IsDeleted == false))
                .ToList();
        }

        public ICollection<AdobeConnectRoom> AllWithActiveSession()
        {
            return Collection()
                .Where(r => r.OfferingSessions.Any(o => o.Offering.StartDate < DateTime.Now && o.Offering.EndDate > DateTime.Now && o.IsDeleted == false))
                .ToList();
        }

        public ICollection<AdobeConnectRoom> AllForOffering(int offeringId)
        {
            return Collection()
                .Where(r => r.OfferingSessions.Any(s => s.OfferingId == offeringId))
                .ToList();
        }

        public async Task<ICollection<AdobeConnectRoom>> ForSelectionAsync()
        {
            return await _context.Rooms
                .Where(room => !room.IsDeleted)
                .OrderBy(room => room.Name)
                .ToListAsync();
        }
    }
}