namespace Constellation.Application.Reports.GetExternalReportList;

using Abstractions.Messaging;
using Core.Models.Reports;
using Core.Models.Reports.Repositories;
using Core.Models.Students;
using Core.Models.Students.Repositories;
using Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetExternalReportListQueryHandler
: IQueryHandler<GetExternalReportListQuery, List<ExternalReportResponse>>
{
    private readonly IReportRepository _reportRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger _logger;

    public GetExternalReportListQueryHandler(
        IReportRepository reportRepository,
        IStudentRepository studentRepository,
        ILogger logger)
    {
        _reportRepository = reportRepository;
        _studentRepository = studentRepository;
        _logger = logger
            .ForContext<GetExternalReportListQuery>();
    }

    public async Task<Result<List<ExternalReportResponse>>> Handle(GetExternalReportListQuery request, CancellationToken cancellationToken)
    {
        List<ExternalReportResponse> responses = new();

        List<ExternalReport> reports = await _reportRepository.GetAllExternalReports(cancellationToken);

        List<Student> students = await _studentRepository.GetCurrentStudents(cancellationToken);

        foreach (ExternalReport report in reports)
        {
            Student student = students.FirstOrDefault(entry => entry.Id == report.StudentId);

            if (student is null)
                continue;

            responses.Add(new(
                report.Id,
                student.Id,
                student.Name,
                report.Type,
                report.IssuedDate));
        }

        return responses;
    }
}
