using Constellation.Application.Features.Jobs.SentralReportSync.Commands;
using Constellation.Application.Features.Jobs.SentralReportSync.Queries;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Jobs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Infrastructure.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Jobs
{
    public class SentralReportSyncJob : ISentralReportSyncJob, IScopedService
    {
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;
        private readonly ILogger<ISentralReportSyncJob> _logger;
        private readonly ISentralGateway _gateway;

        public SentralReportSyncJob(IAppDbContext context, IMediator mediator, ILogger<ISentralReportSyncJob> logger, ISentralGateway gateway)
        {
            _context = context;
            _mediator = mediator;
            _logger = logger;
            _gateway = gateway;
        }

        public async Task StartJob(bool automated)
        {
            if (automated)
            {
                var jobStatus = await _context.JobActivations.FirstOrDefaultAsync(job => job.JobName == nameof(ISentralReportSyncJob));
                if (jobStatus == null || !jobStatus.IsActive)
                {
                    _logger.LogInformation("Stopped due to job being set inactive.");
                    return;
                }
            }

            _logger.LogInformation("Starting Sentral Student Report Scan.");

            //foreach student
            // download list of reports
            // check for existance of report in db
            //  if missing, download and store
            //  if old version present, delete and download and store the new version
            //  if present, skip

            var students = await _mediator.Send(new GetStudentsForReportDownloadQuery());

            _logger.LogInformation("Found {students} students to scan", students.Count());

            foreach (var student in students.OrderBy(student => student.Grade).ThenBy(student => student.LastName))
            {
                _logger.LogInformation("Scanning {student} ({grade})", student.Name, student.Grade);

                var reportList = await _gateway.GetStudentReportList(student.SentralStudentId);

                _logger.LogInformation("Found {reports} reports", reportList.Count());

                foreach (var report in reportList)
                {
                    _logger.LogInformation("Checking report #{publishId} ({reportName})", report.PublishId, report.Name);

                    var existingReport = student.Reports.FirstOrDefault(r => r.Year == report.Year && r.ReportingPeriod == report.Name);

                    if (existingReport == null)
                    {
                        // Does not exist, save it to the database.
                        _logger.LogInformation("Report does not exist in database, adding");

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

                        await _mediator.Send(command);
                    }
                    else if (existingReport.PublishId != report.PublishId)
                    {
                        // Exists in database but is older.
                        // Remove the older version and save the newer version.

                        _logger.LogInformation("Existing version found in database: Old Version {old} - New Version {new}", existingReport.PublishId, report.PublishId);
                        _logger.LogInformation("Replacing existing report");

                        var file = await _gateway.GetStudentReport(student.SentralStudentId, report.PublishId);

                        var command = new ReplaceStudentReportCommand
                        {
                            OldPublishId = existingReport.PublishId,
                            NewPublishId = report.PublishId,
                            File = file
                        };

                        await _mediator.Send(command);
                    }
                    else
                    {
                        _logger.LogInformation("Report already exists and does not need updating");
                    }
                }
            }
        }
    }
}
