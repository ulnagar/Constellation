namespace Constellation.Application.Domains.ScheduledReports.Repositories;

using Models;

public interface IScheduledReportRepository
{
    void Insert(ScheduledReport report);
}
