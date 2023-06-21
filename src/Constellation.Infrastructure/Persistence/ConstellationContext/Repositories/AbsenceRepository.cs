namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Application.Absences.ProvideParentWholeAbsenceExplanation;
using Constellation.Core.Abstractions;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Identifiers;
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
            .FirstOrDefaultAsync(absence => absence.Id == absenceId, cancellationToken);

    public void Insert(Absence absence) =>
        _context.Set<Absence>().Add(absence);

    public async Task<List<Absence>> GetForStudentFromCurrentYear(
        string StudentId,
        CancellationToken cancellationToken = default)
    {
        var startOfYear = new DateOnly(DateTime.Today.Year, 1, 1);
        var endOfYear = new DateOnly(DateTime.Today.Year, 12, 31);

        return await _context
            .Set<Absence>()
            .Where(absence =>
                absence.StudentId == StudentId &&
                absence.Date > startOfYear &&
                absence.Date < endOfYear)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Absence>> GetWithResponsesForStudentFromCurrentYear(
    string StudentId,
    CancellationToken cancellationToken = default)
    {
        var startOfYear = new DateOnly(DateTime.Today.Year, 1, 1);
        var endOfYear = new DateOnly(DateTime.Today.Year, 12, 31);

        return await _context
            .Set<Absence>()
            .Include(absence => absence.Responses)
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
                .Where(absence => absence.Date.Year == DateTime.Today.Year)
            .ToListAsync(cancellationToken);

    public async Task<List<Absence>> GetWholeAbsencesForScanDate(
        DateOnly scanDate,
        CancellationToken cancellationToken = default) =>
            await _context.Set<Absence>()
                .Where(absence =>
                    absence.LastSeen.Date == scanDate.ToDateTime(TimeOnly.MinValue) &&
                    absence.Type == AbsenceType.Whole)
                .ToListAsync(cancellationToken);

    public async Task<int> GetCountForStudentDateAndOffering(
        string studentId,
        DateOnly absenceDate,
        int offeringId,
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
        int offeringId, 
        string absenceTimeframe, 
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<Absence>()
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
        DateTime seenBefore = DateTime.Today.AddDays(-(ageInWeeks * 7));

        return await _context
            .Set<Absence>()
            .Where(absence =>
                absence.StudentId == studentId &&
                !absence.Explained &&
                absence.Type == AbsenceType.Whole &&
                absence.FirstSeen < seenBefore)
            .ToListAsync(cancellationToken);
    }
        
}
