namespace Constellation.Application.Reports.GetCombinedReportListForStudent;

using Abstractions.Messaging;
using Core.Models.Reports;
using Core.Models.Reports.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCombinedReportListForStudentQueryHandler
: IQueryHandler<GetCombinedReportListForStudentQuery, List<ReportResponse>>
{
    private readonly IReportRepository _reportRepository;
    private readonly ILogger _logger;

    public GetCombinedReportListForStudentQueryHandler(
        IReportRepository reportRepository,
        ILogger logger)
    {
        _reportRepository = reportRepository;
        _logger = logger
            .ForContext<GetCombinedReportListForStudentQuery>();
    }

    public async Task<Result<List<ReportResponse>>> Handle(GetCombinedReportListForStudentQuery request, CancellationToken cancellationToken)
    {
        List<ReportResponse> responses = new();

        var academicReports = await _reportRepository.GetAcademicReportsForStudent(request.StudentId, cancellationToken);
        var externalReports = await _reportRepository.GetExternalReportsForStudent(request.StudentId, cancellationToken);

        foreach (AcademicReport report in academicReports)
        {
            responses.Add(new AcademicReportResponse(
                report.Id,
                report.PublishId,
                report.Year,
                report.ReportingPeriod));
        }

        foreach (ExternalReport report in externalReports)
        {
            responses.Add(new ExternalReportResponse(
                report.Id,
                report.Type,
                report.IssuedDate));
        }

        return responses;
    }
}
