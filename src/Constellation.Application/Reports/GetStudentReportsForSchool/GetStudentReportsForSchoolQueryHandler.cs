namespace Constellation.Application.Reports.GetStudentReportsForSchool;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetStudentReportsForSchoolQueryHandler
    : IQueryHandler<GetStudentReportsForSchoolQuery, List<SchoolStudentReportResponse>>
{
    private readonly IStudentRepository _studentRepository;
    private readonly IAcademicReportRepository _reportRepository;

    public GetStudentReportsForSchoolQueryHandler(
        IStudentRepository studentRepository,
        IAcademicReportRepository reportRepository)
    {
        _studentRepository = studentRepository;
        _reportRepository = reportRepository;
    }

    public IStudentRepository StudentRepository { get; }

    public async Task<Result<List<SchoolStudentReportResponse>>> Handle(GetStudentReportsForSchoolQuery request, CancellationToken cancellationToken)
    {
        List<SchoolStudentReportResponse> results = new();

        var students = await _studentRepository.GetCurrentStudentsFromSchool(request.SchoolCode, cancellationToken);

        if (students is null || students.Count == 0)
            return results;

        foreach (var student in students)
        {
            var studentNameRequest = Name.Create(student.FirstName, null, student.LastName);

            if (studentNameRequest.IsFailure)
                continue;

            var reports = await _reportRepository.GetForStudent(student.StudentId, cancellationToken);

            if (reports is null || reports.Count == 0)
                continue;

            foreach (var report in reports)
            {
                results.Add(new(
                    student.StudentId,
                    studentNameRequest.Value.FirstName,
                    studentNameRequest.Value.LastName,
                    studentNameRequest.Value.DisplayName,
                    student.CurrentGrade,
                    report.Id,
                    report.PublishId,
                    report.Year,
                    report.ReportingPeriod));
            }
        }

        return results;
    }
}
