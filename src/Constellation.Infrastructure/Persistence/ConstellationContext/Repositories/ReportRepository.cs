namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Constellation.Core.Models.Reports;
using Constellation.Core.Models.Reports.Identifiers;
using Constellation.Core.Models.Reports.Repositories;
using Core.Models.Reports.Enums;
using Core.Models.Students.Identifiers;
using Microsoft.EntityFrameworkCore;

internal sealed class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext appDbContext)
    {
        _context = appDbContext;
    }

    public async Task<AcademicReport?> GetAcademicReportById(
        AcademicReportId Id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AcademicReport>()
            .FirstOrDefaultAsync(report => report.Id == Id, cancellationToken);

    public async Task<List<AcademicReport>> GetAllAcademicReports(
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AcademicReport>()
            .ToListAsync(cancellationToken);

    public async Task<AcademicReport> GetAcademicReportByPublishId(
        string PublishId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AcademicReport>()
            .Where(report => report.PublishId == PublishId)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<List<AcademicReport>> GetAcademicReportsForStudent(
        StudentId StudentId,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<AcademicReport>()
            .Where(report => report.StudentId == StudentId)
            .ToListAsync(cancellationToken);

    public async Task<List<ExternalReport>> GetAllExternalReports(
        CancellationToken cancellationToken = default) =>
        await _context.Set<ExternalReport>()
            .ToListAsync(cancellationToken);

    public async Task<ExternalReport?> GetExternalReportById(
        ExternalReportId id,
        CancellationToken cancellationToken = default) =>
        await _context.Set<ExternalReport>()
            .SingleOrDefaultAsync(entry => entry.Id == id,
                cancellationToken);

    public async Task<List<ExternalReport>> GetExternalReportsByType(
        ReportType type,
        CancellationToken cancellationToken = default) =>
        await _context.Set<ExternalReport>()
            .Where(entry => entry.Type == type)
            .ToListAsync(cancellationToken);

    public async Task<List<ExternalReport>> GetExternalReportsForStudent(
        StudentId studentId,
        CancellationToken cancellationToken = default) =>
        await _context.Set<ExternalReport>()
            .Where(entry => entry.StudentId == studentId)
            .ToListAsync(cancellationToken);

    public async Task<List<TempExternalReport>> GetTempExternalReports(
        CancellationToken cancellationToken = default) =>
        await _context.Set<TempExternalReport>()
            .ToListAsync(cancellationToken);

    public async Task<TempExternalReport?> GetTempExternalReportById(
        ExternalReportId id,
        CancellationToken cancellationToken = default) =>
        await _context
            .Set<TempExternalReport>()
            .Where(entry => entry.Id == id)
            .SingleOrDefaultAsync(cancellationToken);

    public void Insert(AcademicReport entity) =>
        _context.Add(entity);

    public void Remove(AcademicReport entity) =>
        _context.Remove(entity);


    public void Insert(ExternalReport entity) =>
        _context.Add(entity);

    public void Remove(ExternalReport entity) =>
        _context.Remove(entity);


    public void Insert(TempExternalReport entity) =>
        _context.Add(entity);

    public void Remove(TempExternalReport entity) =>
        _context.Remove(entity);

}
