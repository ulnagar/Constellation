#nullable enable
using Constellation;

namespace Constellation.Core.Abstractions.Repositories;

using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Reports;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public interface IAcademicReportRepository
{
    Task<AcademicReport?> GetById(AcademicReportId id, CancellationToken cancellationToken = default);
    Task<List<AcademicReport>> GetAll(CancellationToken cancellationToken = default);
    Task<List<AcademicReport>> GetForStudent(string StudentId, CancellationToken cancellationToken = default);
    Task<AcademicReport?> GetByPublishId(string PublishId, CancellationToken cancellationToken = default);
    void Insert(AcademicReport entity);
}