namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class OfferingSessionsRepository : IOfferingSessionsRepository
{
    private readonly AppDbContext _context;

    public OfferingSessionsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OfferingSession>> GetAllForStudentAndDayDuringTime(
        string studentId,
        int day,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        DateTime dateTime = date.ToDateTime(TimeOnly.MinValue);

        return await _context
            .Set<OfferingSession>()
            .Where(session =>
                session.Offering.StartDate <= dateTime &&
                session.Offering.EndDate >= dateTime &&
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
        

    private IQueryable<OfferingSession> Collection()
    {
        return _context.Sessions
            .Include(s => s.Offering)
                .ThenInclude(offering => offering.Course)
            .Include(s => s.Period)
            .Include(s => s.Room)
            .Include(s => s.Teacher);
    }

    public async Task<List<OfferingSession>> GetByOfferingId(
        int offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<OfferingSession>()
            .Include(session => session.Period)
            .Where(session => 
                session.OfferingId == offeringId && 
                !session.IsDeleted)
            .ToListAsync(cancellationToken);

    public async Task<List<string>> GetTimetableByOfferingId(
        int offeringId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<OfferingSession>()
            .Where(session => 
                session.OfferingId == offeringId &&
                !session.IsDeleted)
            .Select(session => session.Period.Timetable)
            .Distinct()
            .ToListAsync(cancellationToken);

    public OfferingSession WithDetails(int id)
    {
        return Collection()
            .SingleOrDefault(d => d.Id == id);
    }

    public OfferingSession WithFilter(Expression<Func<OfferingSession, bool>> predicate)
    {
        return Collection()
            .FirstOrDefault(predicate);
    }

    public ICollection<OfferingSession> All()
    {
        return Collection()
            .Where(s => !s.IsDeleted)
            .ToList();
    }

    public ICollection<OfferingSession> AllWithFilter(Expression<Func<OfferingSession, bool>> predicate)
    {
        return Collection()
            .Where(predicate)
            .ToList();
    }

    public ICollection<OfferingSession> AllForOfferingAndRoom(int offeringId, string roomId)
    {
        return Collection()
            .Where(s => !s.IsDeleted)
            .Where(s => s.OfferingId == offeringId && s.RoomId == roomId)
            .ToList();
    }

    public ICollection<OfferingSession> AllForOfferingAndTeacher(int offeringId, string staffId)
    {
        return Collection()
            .Where(s => !s.IsDeleted && s.OfferingId == offeringId && s.StaffId == staffId)
            .ToList();
    }

    public async Task<bool> AnyForOffering(int offeringId)
    {
        return await _context.Sessions
            .AnyAsync(session => !session.IsDeleted && session.OfferingId == offeringId);
    }

    public async Task<bool> AnyForOfferingAndTeacher(int offeringId, string staffId)
    {
        return await _context.Sessions
            .AnyAsync(session => !session.IsDeleted && session.OfferingId == offeringId && session.StaffId == staffId);
    }

    public async Task<bool> AnyForOfferingAndRoom(int offeringId, string roomId)
    {
        return await _context.Sessions
            .AnyAsync(session => !session.IsDeleted && session.OfferingId == offeringId && session.RoomId == roomId);
    }

    public ICollection<OfferingSession> AllForOffering(int id)
    {
        return Collection()
            .Where(s => s.OfferingId == id)
            .Where(s => !s.IsDeleted)
            .ToList();
    }

    public ICollection<OfferingSession> AllForPeriod(int id)
    {
        return Collection()
            .Where(s => s.PeriodId == id)
            .Where(s => !s.IsDeleted)
            .ToList();
    }

    public ICollection<OfferingSession> AllFromFaculty(Faculty faculty)
    {
        return Collection()
            .Where(s => s.Offering.Course.Faculty == faculty)
            .Where(s => !s.IsDeleted)
            .ToList();
    }

    public ICollection<OfferingSession> AllFromGrade(Grade grade)
    {
        return Collection()
            .Where(s => s.Offering.Course.Grade == grade)
            .Where(s => !s.IsDeleted)
            .ToList();
    }

    public async Task<ICollection<OfferingSession>> ForOfferingAndDay(int offeringId, int day)
    {
        return await _context.Sessions
            .Include(s => s.Offering)
            .Include(s => s.Period)
            .Where(s => !s.IsDeleted && s.OfferingId == offeringId && s.Period.Day == day)
            .ToListAsync();
    }

    

    public async Task<ICollection<OfferingSession>> ForOfferingAndPeriod(int offeringId, int periodId)
    {
        return await _context.Sessions
            .Where(session => !session.IsDeleted && session.PeriodId == periodId && session.OfferingId == offeringId)
            .ToListAsync();
    }

    public async Task<OfferingSession> ForExistCheckAsync(int sessionId)
    {
        return await _context.Sessions
            .SingleOrDefaultAsync(session => session.Id == sessionId);
    }

    public async Task<OfferingSession> ForEditAsync(int sessionId)
    {
        return await _context.Sessions
            .SingleOrDefaultAsync(session => session.Id == sessionId);
    }
}