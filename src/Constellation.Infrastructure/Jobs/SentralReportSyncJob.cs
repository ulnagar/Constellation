using Constellation.Application.Features.Jobs.SentralReportSync.Commands;
using Constellation.Application.Features.Jobs.SentralReportSync.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class SentralReportSyncJob : ISentralReportSyncJob, IScopedService, IHangfireJob
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ISentralReportSyncJob> _logger;
        private readonly ISentralGateway _gateway;

        public SentralReportSyncJob(IMediator mediator, ILogger<ISentralReportSyncJob> logger, ISentralGateway gateway)
        {
            _mediator = mediator;
            _logger = logger;
            _gateway = gateway;
        }

        public async Task StartJob(Guid jobId, CancellationToken token)
        { 
            _logger.LogInformation("{id}: Starting Sentral Student Report Scan.", jobId);

            //foreach student
            // download list of reports
            // check for existance of report in db
            //  if missing, download and store
            //  if old version present, delete and download and store the new version
            //  if present, skip

            var students = await _mediator.Send(new GetStudentsForReportDownloadQuery(), token);

            _logger.LogInformation("{id}: Found {students} students to scan", jobId, students.Count());

            foreach (var student in students.OrderBy(student => student.Grade).ThenBy(student => student.LastName))
            {
                if (token.IsCancellationRequested)
                    return;

                _logger.LogInformation("{id}: Scanning {student} ({grade})", jobId, student.Name, student.Grade);

                var reportList = await _gateway.GetStudentReportList(student.SentralStudentId);

                _logger.LogInformation("{id}: Found {reports} reports for {student} ({grade})", jobId, reportList.Count(), student.Name, student.Grade);

                foreach (var report in reportList)
                {
                    if (token.IsCancellationRequested)
                        return;

                    _logger.LogInformation("{id}: Checking report #{publishId} ({reportName}) for {student} ({grade})", jobId, report.PublishId, report.Name, student.Name, student.Grade);

                    var existingReport = student.Reports.FirstOrDefault(r => r.Year == report.Year && r.ReportingPeriod == report.Name);

                    if (existingReport == null)
                    {
                        // Does not exist, save it to the database.
                        _logger.LogInformation("{id}: Report #{publishId} ({reportName}) for {student} ({grade}) does not exist in database, adding", jobId, report.PublishId, report.Name, student.Name, student.Grade);

                        var file = await _gateway.GetStudentReport(student.SentralStudentId, report.PublishId);

                        var command = new UploadStudentReportCommand
                        {
                            StudentId = student.StudentId,
                            PublishId = report.PublishId,
                            Year = report.Year,
                            ReportingPeriod = report.Name,
                            File = file,
                            FileName = $"{student.LastName}, {student.FirstName} - {report.Name}.pdf"
                        };

                        await _mediator.Send(command, token);
                    }
                    else if (existingReport.PublishId != report.PublishId)
                    {
                        // Exists in database but is older.
                        // Remove the older version and save the newer version.

                        _logger.LogInformation("{id}: Existing version of report #{publishId} ({reportName}) for {student} ({grade}) found in database and will be replaced: Old Version {old} - New Version {new}", jobId, report.PublishId, report.Name, student.Name, student.Grade, existingReport.PublishId, report.PublishId);

                        var file = await _gateway.GetStudentReport(student.SentralStudentId, report.PublishId);

                        var command = new ReplaceStudentReportCommand
                        {
                            OldPublishId = existingReport.PublishId,
                            NewPublishId = report.PublishId,
                            File = file
                        };

                        await _mediator.Send(command, token);
                    }
                    else
                    {
                        _logger.LogInformation("{id}: Report #{publishId} ({reportName}) for {student} ({grade}) already exists and does not need updating", jobId, report.PublishId, report.Name, student.Name, student.Grade);
                    }
                }
            }
        }
    }
}
