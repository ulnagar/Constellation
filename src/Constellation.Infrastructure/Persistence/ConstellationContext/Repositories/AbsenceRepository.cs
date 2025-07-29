namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences.Enums;
using Constellation.Core.Models.Absences.Identifiers;
using ConstellationContext;
using Core.Abstractions.Clock;
using Core.Models.Absences;
using Core.Models.Identifiers;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AbsenceRepository : IAbsenceRepository
{
    private readonly AppDbContext _context;
    private readonly IDateTimeProvider _dateTime;

    public AbsenceRepository(
        AppDbContext context,
        IDateTimeProvider dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<Absence> GetById(
        AbsenceId absenceId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Absence>()
            .Include(absence => absence.Responses)
            .Include(absence => absence.Notifications)
            .FirstOrDefaultAsync(absence => absence.Id == absenceId, cancellationToken);

    public void Insert(Absence absence) =>
        _context
            .Set<Absence>()
            .Add(absence);

    public async Task<List<Absence>> GetForStudentFromCurrentYear(
        StudentId studentId,
        CancellationToken cancellationToken = default)
    {
        var startOfYear = new DateOnly(DateTime.Today.Year, 1, 1);
        var endOfYear = new DateOnly(DateTime.Today.Year, 12, 31);

        return await _context
            .Set<Absence>()
            .Include(absence => absence.Responses)
            .Include(absence => absence.Notifications)
            .Where(absence =>
                absence.StudentId == studentId &&
                absence.Date > startOfYear &&
                absence.Date < endOfYear)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Absence>> GetAllFromCurrentYear(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Absence>()
            .Include(absence => absence.Responses)
            .Include(absence => absence.Notifications)
            .Where(absence => absence.Date.Year == DateTime.Today.Year)
            .ToListAsync(cancellationToken);

    public async Task<List<Absence>> GetWholeAbsencesForScanDate(
        DateOnly scanDate,
        CancellationToken cancellationToken = default) =>
            await _context
                .Set<Absence>()
                .Include(absence => absence.Responses)
                .Include(absence => absence.Notifications)
                .Where(absence =>
                    absence.LastSeen.Date == scanDate.ToDateTime(TimeOnly.MinValue) &&
                    absence.Type == AbsenceType.Whole)
                .ToListAsync(cancellationToken);

    public async Task<List<Absence>> GetUnexplainedPartialAbsences(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Absence>()
            .Where(absence => 
                absence.Type == AbsenceType.Partial &&
                absence.Date > _dateTime.FirstDayOfYear &&
                !absence.Explained)
            .ToListAsync(cancellationToken);

    public async Task<int> GetCountForStudentDateAndOffering(
        StudentId studentId,
        DateOnly absenceDate,
        OfferingId offeringId,
        string absenceTimeframe,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Absence>()
            .CountAsync(absence =>
                absence.StudentId == studentId &&
                absence.Date == absenceDate &&
                absence.OfferingId == offeringId &&
                absence.AbsenceTimeframe == absenceTimeframe,
                cancellationToken);

    public async Task<List<Absence>> GetAllForStudentDateAndOffering(
        StudentId studentId, 
        DateOnly absenceDate,
        OfferingId offeringId, 
        string absenceTimeframe, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Absence>()
            .Include(absence => absence.Responses)
            .Include(absence => absence.Notifications)
            .Where(absence =>
                absence.StudentId == studentId &&
                absence.Date == absenceDate &&
                absence.OfferingId == offeringId &&
                absence.AbsenceTimeframe == absenceTimeframe)
            .ToListAsync(cancellationToken);

    public async Task<List<Absence>> GetUnexplainedWholeAbsencesForStudentWithDelay(
        StudentId studentId,
        int ageInWeeks,
        CancellationToken cancellationToken = default)
    {
        DateTime seenAfter = DateTime.Today.AddDays(-(ageInWeeks * 7));
        DateTime seenBefore = DateTime.Today.AddDays(-(ageInWeeks - 1) * 7 - 1);
        
        return await _context
            .Set<Absence>()
            .Include(absence => absence.Responses)
            .Include(absence => absence.Notifications)
            .Where(absence =>
                absence.Date > _dateTime.FirstDayOfYear && // Absence was this year
                absence.StudentId == studentId && // Absence was for this student
                !absence.Explained && // Absence has not been marked explained
                absence.Type == AbsenceType.Whole && // Absence is a Whole absence
                absence.FirstSeen >= seenAfter && // Absence was first recorded on or after the cut off date
                absence.FirstSeen <= seenBefore) // Absence was first recorded on or before the cut off date
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Absence>> GetUnexplainedPartialAbsencesForStudentWithDelay(
        StudentId studentId,
        int ageInWeeks,
        CancellationToken cancellationToken = default)
    {
        DateTime seenAfter = DateTime.Today.AddDays(-(ageInWeeks * 7));
        DateTime seenBefore = DateTime.Today.AddDays(-(ageInWeeks - 1) * 7 - 1);

        return await _context
            .Set<Absence>()
            .Where(absence =>
                absence.Date > _dateTime.FirstDayOfYear &&
                absence.StudentId == studentId &&
                !absence.Explained &&
                absence.Type == AbsenceType.Partial &&
                absence.FirstSeen >= seenAfter &&
                absence.FirstSeen <= seenBefore &&
                absence.Responses.All(response =>
                    response.Type != ResponseType.Student))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Absence>> GetUnverifiedPartialAbsencesForStudentWithDelay(
        StudentId studentId,
        int ageInWeeks,
        CancellationToken cancellationToken = default)
    {
        DateTime seenAfter = DateTime.Today.AddDays(-(ageInWeeks * 7));
        DateTime seenBefore = DateTime.Today.AddDays(-(ageInWeeks - 1) * 7 - 1);

        return await _context
            .Set<Absence>()
            .Where(absence =>
                absence.Date > _dateTime.FirstDayOfYear &&
                absence.StudentId == studentId &&
                !absence.Explained &&
                absence.Type == AbsenceType.Partial &&
                absence.Responses.Any(response =>
                    response.Type == ResponseType.Student &&
                    response.VerificationStatus == ResponseVerificationStatus.Pending &&
                    response.ReceivedAt >= seenAfter &&
                    response.ReceivedAt <= seenBefore))
            .ToListAsync(cancellationToken);
    }
    public async Task<List<Absence>> GetForStudentFromDateRange(
        StudentId studentId, 
        DateOnly startDate, 
        DateOnly endDate, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Absence>()
            .Include(absence => absence.Responses)
            .Include(absence => absence.Notifications)
            .Where(absence =>
                absence.StudentId == studentId &&
                absence.Date >= startDate &&
                absence.Date <= endDate)
            .ToListAsync(cancellationToken);

    public async Task<List<Absence>> GetForStudents(
        List<StudentId> studentIds,
        CancellationToken cancellationToken = default)
    {
        DateOnly startOfYear = new(DateTime.Today.Year, 1, 1);
        DateOnly endOfYear = new(DateTime.Today.Year, 12, 31);

        return await _context
            .Set<Absence>()
            .Include(absence => absence.Responses)
            .Include(absence => absence.Notifications)
            .Where(absence =>
                studentIds.Contains(absence.StudentId) &&
                absence.Date >= startOfYear &&
                absence.Date <= endOfYear)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Absence>> GetStudentWholeAbsencesForDateRange(
        StudentId studentId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Absence>()
            .AsNoTracking()
            .Where(absence =>
                absence.Type == AbsenceType.Whole &&
                absence.StudentId == studentId &&
                absence.Date >= startDate &&
                absence.Date <= endDate)
            .ToListAsync(cancellationToken);
    
}
