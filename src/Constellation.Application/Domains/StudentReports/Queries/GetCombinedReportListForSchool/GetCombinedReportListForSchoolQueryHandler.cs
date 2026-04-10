namespace Constellation.Application.Domains.StudentReports.Queries.GetCombinedReportListForSchool;

using Abstractions.Messaging;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Students.Repositories;
using Core.Models.Reports;
using Core.Models.Reports.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetCombinedReportListForSchoolQueryHandler
: IQueryHandler<GetCombinedReportListForSchoolQuery, List<SchoolReportResponse>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetCombinedReportListForSchoolQueryHandler(
        IReportRepository reportRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _reportRepository = reportRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetCombinedReportListForSchoolQuery>();
    }

    public async Task<Result<List<SchoolReportResponse>>> Handle(GetCombinedReportListForSchoolQuery request,
        CancellationToken cancellationToken)
    {
        List<SchoolReportResponse> results = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

        if (students.Count == 0)
            return results;

        foreach (Student student in students)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            List<AcademicReport> academicReports = await _reportRepository.GetAcademicReportsForStudent(student.Id, cancellationToken);
            List<ExternalReport> externalReports = await _reportRepository.GetExternalReportsForStudent(student.Id, cancellationToken);

            foreach (AcademicReport report in academicReports)
            {
                results.Add(new SchoolAcademicReportResponse(
                    student.StudentReferenceNumber,
                    student.Name.FirstName,
                    student.Name.LastName,
                    student.Name.DisplayName,
                    enrolment.Grade,
                    report.Id,
                    report.PublishId,
                    report.Year,
                    report.ReportingPeriod));
            }

            foreach (ExternalReport report in externalReports)
            {
                results.Add(new SchoolExternalReportResponse(
                    student.StudentReferenceNumber,
                    student.Name.FirstName,
                    student.Name.LastName,
                    student.Name.DisplayName,
                    enrolment.Grade,
                    report.Id,
                    report.Type,
                    report.IssuedDate));
            }
        }

        return results;
    }
}
