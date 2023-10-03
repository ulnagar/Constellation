namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Infrastructure.Persistence.ConstellationContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AbsenceRepository : IAbsenceRepository
{
    private readonly AppDbContext _context;

    public AbsenceRepository(AppDbContext context)
    {
        _context = context;
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
        string StudentId,
        CancellationToken cancellationToken = default)
    {
        var startOfYear = new DateOnly(DateTime.Today.Year, 1, 1);
        var endOfYear = new DateOnly(DateTime.Today.Year, 12, 31);

        return await _context
            .Set<Absence>()
            .Include(absence => absence.Responses)
            .Include(absence => absence.Notifications)
            .Where(absence =>
                absence.StudentId == StudentId &&
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

    public async Task<int> GetCountForStudentDateAndOffering(
        string studentId,
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
        string studentId, 
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
        string studentId,
        int ageInWeeks,
        CancellationToken cancellationToken = default)
    {
        DateTime seenAfter = DateTime.Today.AddDays(-(ageInWeeks * 7));
        DateTime seenBefore = DateTime.Today.AddDays(-(ageInWeeks - 1) * 7 - 1);

        DateOnly startOfYear = new DateOnly(DateTime.Today.Year, 1, 1);

        return await _context
            .Set<Absence>()
            .Include(absence => absence.Responses)
            .Include(absence => absence.Notifications)
            .Where(absence =>
                absence.Date > startOfYear &&
                absence.StudentId == studentId &&
                !absence.Explained &&
                absence.Type == AbsenceType.Whole &&
                absence.FirstSeen <= seenAfter &&
                absence.FirstSeen >= seenBefore)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Absence>> GetForStudentFromDateRange(
        string studentId, 
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
        List<string> studentIds,
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
        
}
