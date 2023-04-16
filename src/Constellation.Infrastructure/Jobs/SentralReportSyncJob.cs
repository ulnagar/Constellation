namespace Constellation.Infrastructure.Jobs;

using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Reports.CreateNewStudentReport;
using Constellation.Application.Reports.GetAcademicReportsForStudent;
using Constellation.Application.Reports.ReplaceStudentReport;
using Constellation.Application.Students.GetCurrentStudentsWithSentralId;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class SentralReportSyncJob : ISentralReportSyncJob, IHangfireJob
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;
    private readonly ISentralGateway _gateway;

    public SentralReportSyncJob(
        IMediator mediator, 
        Serilog.ILogger logger, 
        ISentralGateway gateway)
    {
        _mediator = mediator;
        _logger = logger.ForContext<ISentralReportSyncJob>();
        _gateway = gateway;
    }

    public async Task StartJob(Guid jobId, CancellationToken token)
    { 
        _logger.Information("{id}: Starting Sentral Student Report Scan.", jobId);

        //foreach student
        // download list of reports
        // check for existance of report in db
        //  if missing, download and store
        //  if old version present, delete and download and store the new version
        //  if present, skip

        var studentsRequest = await _mediator.Send(new GetCurrentStudentsWithSentralIdQuery(), token);

        var students = studentsRequest.IsSuccess ? studentsRequest.Value : null;

        _logger.Information("{id}: Found {students} students to scan", jobId, students.Count);

        foreach (var student in students)
        {
            if (token.IsCancellationRequested)
                return;

            _logger.Information("{id}: Scanning {student} ({grade})", jobId, student.Name.DisplayName, student.Grade.AsName());

            var reportList = await _gateway.GetStudentReportList(student.SentralId);
            var existingReportsRequest = await _mediator.Send(new GetAcademicReportsForStudentQuery(student.StudentId), token);

            if (existingReportsRequest.IsFailure)
                continue;

            var existingReports = existingReportsRequest.Value;

            _logger.Information("{id}: Found {reports} reports for {student} ({grade})", jobId, reportList.Count, student.Name, student.Grade);

            foreach (var report in reportList)
            {
                if (token.IsCancellationRequested)
                    return;

                _logger.Information("{id}: Checking report #{publishId} ({reportName}) for {student} ({grade})", jobId, report.PublishId, report.Name, student.Name, student.Grade);

                var existingReport = existingReports.FirstOrDefault(r => r.Year == report.Year && r.ReportingPeriod == report.Name);

                if (existingReport == null)
                {
                    // Does not exist, save it to the database.
                    _logger.Information("{id}: Report #{publishId} ({reportName}) for {student} ({grade}) does not exist in database, adding", jobId, report.PublishId, report.Name, student.Name, student.Grade);

                    var file = await _gateway.GetStudentReport(student.SentralId, report.PublishId);

                    var command = new CreateNewStudentReportCommand(
                        student.StudentId,
                        report.PublishId,
                        report.Year,
                        report.Name,
                        file,
                        $"{student.Name.LastName}, {student.Name.FirstName} - {report.Name}.pdf");

                    await _mediator.Send(command, token);
                }
                else if (existingReport.PublishId != report.PublishId)
                {
                    // Exists in database but is older.
                    // Remove the older version and save the newer version.

                    _logger.Information("{id}: Existing version of report #{publishId} ({reportName}) for {student} ({grade}) found in database and will be replaced: Old Version {old} - New Version {new}", jobId, report.PublishId, report.Name, student.Name, student.Grade, existingReport.PublishId, report.PublishId);

                    var file = await _gateway.GetStudentReport(student.SentralId, report.PublishId);

                    var command = new ReplaceStudentReportCommand(
                        existingReport.PublishId,
                        report.PublishId,
                        file);

                    await _mediator.Send(command, token);
                }
                else
                {
                    _logger.Information("{id}: Report #{publishId} ({reportName}) for {student} ({grade}) already exists and does not need updating", jobId, report.PublishId, report.Name, student.Name, student.Grade);
                }
            }
        }
    }
}
