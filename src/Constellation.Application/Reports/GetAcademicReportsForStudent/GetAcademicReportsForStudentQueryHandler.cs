namespace Constellation.Application.Reports.GetAcademicReportsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAcademicReportsForStudentQueryHandler
    : IQueryHandler<GetAcademicReportsForStudentQuery, List<StudentReportResponse>>
{
    private readonly IAcademicReportRepository _reportRepository;

    public GetAcademicReportsForStudentQueryHandler(
        IAcademicReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<List<StudentReportResponse>>> Handle(GetAcademicReportsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentReportResponse> results = new();
        
        var reports = await _reportRepository.GetForStudent(request.StudentId, cancellationToken);

        if (reports is null || reports.Count == 0)
            return results;

        foreach (var report in reports)
        {
            results.Add(new(
                request.StudentId,
                report.PublishId,
                report.Year,
                report.ReportingPeriod));
        }

        return results;
    }
}
