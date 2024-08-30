#nullable enable
namespace Constellation.Core.Abstractions.Repositories;

using Models.Identifiers;
using Models.Reports;
using Models.Students.Identifiers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAcademicReportRepository
{
    Task<AcademicReport?> GetById(AcademicReportId id, CancellationToken cancellationToken = default);
    Task<List<AcademicReport>> GetAll(CancellationToken cancellationToken = default);
    Task<List<AcademicReport>> GetForStudent(StudentId studentId, CancellationToken cancellationToken = default);
    Task<AcademicReport?> GetByPublishId(string PublishId, CancellationToken cancellationToken = default);
    void Insert(AcademicReport entity);
}