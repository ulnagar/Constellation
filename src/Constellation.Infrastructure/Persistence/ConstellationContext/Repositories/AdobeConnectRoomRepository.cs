namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Microsoft.EntityFrameworkCore;
using ResourceType = Core.Models.Offerings.ValueObjects.ResourceType;

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
            .OrderBy(r => r.Name);
    }

    public async Task<List<AdobeConnectRoom>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AdobeConnectRoom>()
            .Where(room => !room.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<AdobeConnectRoom>> GetByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default)
    {
        List<string> scoIds = await _context
            .Set<Offering>()
            .Where(offering => offering.Id == offeringId)
            .SelectMany(offering => offering.Resources)
            .Where(resource => resource.Type == ResourceType.AdobeConnectRoom)
            .Select(resource => resource as AdobeConnectRoomResource)
            .Select(resource => resource.ScoId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return await _context
            .Set<AdobeConnectRoom>()
            .Where(room => scoIds.Contains(room.ScoId))
            .ToListAsync(cancellationToken);
    }

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

    public async Task<AdobeConnectRoom> GetForExistCheck(string id)
    {
        return await _context.Rooms
            .SingleOrDefaultAsync(room => room.ScoId == id);
    }

    public ICollection<AdobeConnectRoom> AllActive()
    {
        return Collection()
            .Where(r => r.IsDeleted == false)
            .ToList();
    }
}