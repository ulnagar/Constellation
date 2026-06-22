namespace Constellation.Infrastructure.Persistence.ConstellationContext.Repositories;

using Application.Domains.ScheduledReports;
using Application.Domains.ScheduledReports.Models;
using Application.Domains.ScheduledReports.Repositories;

internal sealed class ScheduledReportRepository : IScheduledReportRepository
{
    private readonly ConstellationDbContext _context;

    public ScheduledReportRepository(
        ConstellationDbContext context)
    {
        _context = context;
    }

    public void Insert(ScheduledReport report) => _context.Set<ScheduledReport>().Add(report);
}
