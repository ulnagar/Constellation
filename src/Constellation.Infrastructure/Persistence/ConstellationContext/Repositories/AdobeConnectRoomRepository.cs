namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class AdobeConnectRoomRepository : IAdobeConnectRoomRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public AdobeConnectRoomRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
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
        OfferingId offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AdobeConnectRoom>()
            .Where(room => 
                room.OfferingSessions.Any(session => 
                    session.OfferingId == offeringId && 
                    !session.IsDeleted))
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<AdobeConnectRoom> GetById(
        string Id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AdobeConnectRoom>()
            .FirstOrDefaultAsync(room => room.ScoId == Id, cancellationToken);

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

    public ICollection<AdobeConnectRoom> AllFromSession(Session session)
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
            .Where(r => r.OfferingSessions.Any(o => o.Offering.StartDate <= _dateTime.Today && o.Offering.EndDate >= _dateTime.Today && o.IsDeleted == false))
            .ToList();
    }

    public ICollection<AdobeConnectRoom> AllForOffering(OfferingId offeringId)
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