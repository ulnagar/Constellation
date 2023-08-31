namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Offerings;
using Constellation.Core.Models.Offerings.Identifiers;
using Microsoft.EntityFrameworkCore;

public class OfferingSessionsRepository : IOfferingSessionsRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public OfferingSessionsRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Session> GetById(
        int id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Session>()
            .FirstOrDefaultAsync(session => session.Id == id, cancellationToken);

    public async Task<List<Session>> GetAllForStudentAndDayDuringTime(
        string studentId,
        int day,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        DateTime dateTime = date.ToDateTime(TimeOnly.MinValue);

        return await _context
            .Set<Session>()
            .Where(session =>
                session.Offering.StartDate <= date &&
                session.Offering.EndDate >= date &&
                session.Offering.Enrolments.Any(enrol =>
                    enrol.StudentId == studentId &&
                    enrol.DateCreated <= dateTime &&
                    (!enrol.DateDeleted.HasValue || enrol.DateDeleted.Value >= dateTime)) &&
                session.DateCreated <= dateTime &&
                (!session.DateDeleted.HasValue || session.DateDeleted.Value >= dateTime) &&
                session.Period.Day == day &&
                session.Period.Type != "Other")
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Session>> GetByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Session>()
            .Include(session => session.Period)
            .Where(session => 
                session.OfferingId == offeringId && 
                !session.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<string>> GetTimetableByOfferingId(
        OfferingId offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Session>()
            .Where(session => 
                session.OfferingId == offeringId &&
                !session.IsDeleted)
            .Select(session => session.Period.Timetable)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<bool> AnyCurrentForOfferingAndTeacher(
        OfferingId offeringId,
        string staffId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Session>()
            .AnyAsync(session =>
                session.OfferingId == offeringId &&
                session.StaffId == staffId &&
                !session.IsDeleted,
                cancellationToken);

    public async Task<bool> AnyCurrentForOfferingAndRoom(
        OfferingId offeringId,
        string roomId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Session>()
            .AnyAsync(session =>
                session.OfferingId == offeringId &&
                session.RoomId == roomId &&
                !session.IsDeleted,
                cancellationToken);

    public async Task<bool> AnyCurrentForOffering(
        OfferingId offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Session>()
            .AnyAsync(session =>
                session.OfferingId == offeringId &&
                !session.IsDeleted,
                cancellationToken);
}