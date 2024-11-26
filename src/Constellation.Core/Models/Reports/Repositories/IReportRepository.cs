#nullable enable
namespace Constellation.Core.Models.Reports.Repositories;

using Enums;
using Identifiers;
using Models.Students.Identifiers;
using Reports;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IReportRepository
{
    Task<AcademicReport?> GetAcademicReportById(AcademicReportId id, CancellationToken cancellationToken = default);
    Task<List<AcademicReport>> GetAllAcademicReports(CancellationToken cancellationToken = default);
    Task<List<AcademicReport>> GetAcademicReportsForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<AcademicReport?> GetAcademicReportByPublishId(string PublishId, CancellationToken cancellationToken = default);

    Task<List<ExternalReport>> GetAllExternalReports(CancellationToken cancellationToken = default);
    Task<ExternalReport?> GetExternalReportById(ExternalReportId id, CancellationToken cancellationToken = default);
    Task<List<ExternalReport>> GetExternalReportsByType(ReportType type, CancellationToken cancellationToken = default);
    Task<List<ExternalReport>> GetExternalReportsForStudent(StudentId studentId, CancellationToken cancellationToken = default);

    void Insert(AcademicReport entity);
    void Insert(ExternalReport entity);
    void Remove(AcademicReport entity);
    void Remove(ExternalReport entity);
}