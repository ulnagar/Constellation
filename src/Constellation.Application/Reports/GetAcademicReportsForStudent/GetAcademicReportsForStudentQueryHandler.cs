﻿namespace Constellation.Application.Reports.GetAcademicReportsForStudent;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Reports.Repositories;
using Constellation.Core.Shared;
using Core.Models.Reports;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAcademicReportsForStudentQueryHandler
    : IQueryHandler<GetAcademicReportsForStudentQuery, List<StudentReportResponse>>
{
    private readonly IReportRepository _reportRepository;

    public GetAcademicReportsForStudentQueryHandler(
        IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<List<StudentReportResponse>>> Handle(GetAcademicReportsForStudentQuery request, CancellationToken cancellationToken)
    {
        List<StudentReportResponse> results = new();
        
        List<AcademicReport> reports = await _reportRepository.GetAcademicReportsForStudent(request.StudentId, cancellationToken);

        if (reports.Count == 0)
            return results;

        foreach (AcademicReport report in reports)
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
