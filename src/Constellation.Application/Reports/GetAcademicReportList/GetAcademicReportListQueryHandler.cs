namespace Constellation.Application.Reports.GetAcademicReportList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAcademicReportListQueryHandler
    : IQueryHandler<GetAcademicReportListQuery, List<AcademicReportResponse>>
{
    private readonly IAcademicReportRepository _reportRepository;

    public GetAcademicReportListQueryHandler(
        IAcademicReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<List<AcademicReportResponse>>> Handle(GetAcademicReportListQuery request, CancellationToken cancellationToken)
    {
        List<AcademicReportResponse> results = new();
        
        var reports = await _reportRepository.GetAll(cancellationToken);

        foreach (var report in reports)
        {
            results.Add(new(
                report.Id,
                report.PublishId,
                report.Year,
                report.ReportingPeriod));
        }

        return results;
    }
}
