﻿namespace Constellation.Application.Domains.StudentReports.Queries.GetAcademicReportList;

using Abstractions.Messaging;
using Core.Models.Reports.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAcademicReportListQueryHandler
    : IQueryHandler<GetAcademicReportListQuery, List<AcademicReportResponse>>
{
    private readonly IReportRepository _reportRepository;

    public GetAcademicReportListQueryHandler(
        IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<List<AcademicReportResponse>>> Handle(GetAcademicReportListQuery request, CancellationToken cancellationToken)
    {
        List<AcademicReportResponse> results = new();
        
        var reports = await _reportRepository.GetAcademicReportsForStudent(request.StudentId, cancellationToken);

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
