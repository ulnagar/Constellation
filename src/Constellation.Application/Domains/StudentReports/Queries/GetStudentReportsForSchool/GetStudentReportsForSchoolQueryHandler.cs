namespace Constellation.Application.Domains.StudentReports.Queries.GetStudentReportsForSchool;

using Abstractions.Messaging;
using Core.Models.Reports;
using Core.Models.Reports.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentReportsForSchoolQueryHandler
    : IQueryHandler<GetStudentReportsForSchoolQuery, List<SchoolStudentReportResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IReportRepository _reportRepository;

    public GetStudentReportsForSchoolQueryHandler(
        IStudentRepository studentRepository,
        IReportRepository reportRepository)
    {
        _studentRepository = studentRepository;
        _reportRepository = reportRepository;
    }
    
    public async Task<Result<List<SchoolStudentReportResponse>>> Handle(GetStudentReportsForSchoolQuery request, CancellationToken cancellationToken)
    {
        List<SchoolStudentReportResponse> results = new();

        List<Student> students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

        if (students is null || students.Count == 0)
            return results;

        foreach (Student student in students)
        {
            SchoolEnrolment? enrolment = student.CurrentEnrolment;

            if (enrolment is null)
                continue;

            List<AcademicReport> reports = await _reportRepository.GetAcademicReportsForStudent(student.Id, cancellationToken);

            if (reports is null || reports.Count == 0)
                continue;

            foreach (AcademicReport report in reports)
            {
                results.Add(new(
                    student.StudentReferenceNumber.Number,
                    student.Name.FirstName,
                    student.Name.LastName,
                    student.Name.DisplayName,
                    enrolment.Grade,
                    report.Id,
                    report.PublishId,
                    report.Year,
                    report.ReportingPeriod));
            }
        }

        return results;
    }
}
