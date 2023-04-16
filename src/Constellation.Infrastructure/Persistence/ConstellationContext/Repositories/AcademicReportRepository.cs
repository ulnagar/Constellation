namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Abstractions;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Reports;
using Microsoft.EntityFrameworkCore;

internal sealed class AcademicReportRepository : IAcademicReportRepository
{
    private readonly AppDbContext _context;

    public AcademicReportRepository(AppDbContext appDbContext)
    {
        _context = appDbContext;
    }

    public async Task<AcademicReport?> GetById(
        AcademicReportId Id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AcademicReport>()
            .FirstOrDefaultAsync(report => report.Id == Id, cancellationToken);

    public async Task<List<AcademicReport>> GetAll(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AcademicReport>()
            .ToListAsync(cancellationToken);

    public async Task<AcademicReport> GetByPublishId(
        string PublishId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AcademicReport>()
            .Where(report => report.PublishId == PublishId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<AcademicReport>> GetForStudent(
        string StudentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AcademicReport>()
            .Where(report => report.StudentId == StudentId)
            .ToListAsync(cancellationToken);

    public void Insert(AcademicReport entity) =>
        _context.Add(entity);
}
