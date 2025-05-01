namespace Constellation.Application.Domains.StudentReports.Queries.GetAllAcademicReports;

using Abstractions.Messaging;
using Core.Models.Reports;
using Core.Models.Reports.Repositories;
using Core.Models.Students;
using Core.Models.Students.Identifiers;
using Core.Models.Students.Repositories;
using Core.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllAcademicReportsQueryHandler
: IQueryHandler<GetAllAcademicReportsQuery, List<AcademicReportSummary>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IStudentRepository _studentRepository;

    public GetAllAcademicReportsQueryHandler(
        IReportRepository reportRepository,
        IStudentRepository studentRepository)
    {
        _reportRepository = reportRepository;
        _studentRepository = studentRepository;
    }

    public async Task<Result<List<AcademicReportSummary>>> Handle(GetAllAcademicReportsQuery request, CancellationToken cancellationToken)
    {
        List<AcademicReportSummary> responses = new();

        List<AcademicReport> reports = await _reportRepository.GetAllAcademicReports(cancellationToken);

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        IEnumerable<IGrouping<StudentId, AcademicReport>> groupedReports = reports.GroupBy(entry => entry.StudentId);

        foreach (IGrouping<StudentId, AcademicReport> group in groupedReports)
        {
            Student? student = students.FirstOrDefault(entry => entry.Id == group.Key);

            if (student is null)
                continue;

            foreach (AcademicReport report in group)
            {
                responses.Add(new(
                    report.Id,
                    report.StudentId,
                    student.Name,
                    report.Year,
                    report.ReportingPeriod));
            }
        }

        return responses;
    }
}
