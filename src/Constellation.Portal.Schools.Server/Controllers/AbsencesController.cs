namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.Features.Attendance.Queries;
using Constellation.Application.Features.Portal.School.Absences.Commands;
using Constellation.Application.Features.Portal.School.Absences.Models;
using Constellation.Application.Features.Portal.School.Absences.Queries;
using Constellation.Core.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Net.Mime;
using static Constellation.Portal.Schools.Client.Pages.Absences.Report;

[Route("api/[controller]")]
public class AbsencesController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public AbsencesController(IMediator mediator, Serilog.ILogger logger)
    {
        _mediator = mediator;
        _logger = logger.ForContext<AbsencesController>();
    }

    [HttpGet("{schoolCode}/All")]
    public async Task<List<AbsenceForPortalList>> GetForSchool(string schoolCode, CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absences for school {code} by user {user}", schoolCode, user.DisplayName);

        var students = await _mediator.Send(new GetUnProcessedAbsencesFromSchoolQuery { SchoolCode = schoolCode }, cancellationToken);

        return students.ToList();
    }

    [HttpGet("Whole/{absenceId:guid}")]
    public async Task<WholeAbsenceForSchoolExplanation> GetWholeAbsenceForExplanation(Guid absenceId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absence for explanation by user {user} with id {id}", user.DisplayName, absenceId.ToString());

        return await _mediator.Send(new GetAbsenceForSchoolExplanationQuery { Id = absenceId });
    }

    [HttpPost("Whole/{absenceId:guid}/Explain")]
    public async Task ExplainWholeAbsence([FromQuery] Guid AbsenceId, [FromBody] ProvideSchoolAbsenceExplanationCommand Command)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to explain absence by user {user} with details {@absence}", user.DisplayName, Command);

        await _mediator.Send(Command);
    }

    [HttpGet("Partial/{responseId:guid}")]
    public async Task<PartialAbsenceResponseForVerification> GetPartialAbsenceForVerification(Guid responseId)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absence for verification by user {user} with id {id}", user.DisplayName, responseId.ToString());

        return await _mediator.Send(new GetAbsenceResponseForVerificationQuery { Id = responseId });
    }

    [HttpPost("Partial/{responseId:guid}/Verify")]
    public async Task VerifyPartialAbsence([FromQuery] Guid ResponseId, [FromBody] VerifyAbsenceResponseCommand Command)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to verify absence explanation by user {user} with details {@absence}", user.DisplayName, Command);

        await _mediator.Send(Command);
    }

    [HttpPost("Partial/{responseId:guid}/Reject")]
    public async Task RejectPartialAbsence([FromQuery] Guid ResponseId, [FromBody] RejectAbsenceResponseCommand Command)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to reject absence explanation by user {user} with details {@absence}", user.DisplayName, Command);

        await _mediator.Send(Command);
    }

    [HttpPost("Report")]
    public async Task<IActionResult> GenerateAttendanceReports([FromBody] AttendanceReportSelectForm Request)
    {
        if (Request.Students.Count > 1)
        {
            var reports = new List<StoredFile>();

            // We need to loop each student id and collate the report into a zip file.
            foreach (var student in Request.Students)
            {
                var report = await _mediator.Send(new GetStudentAttendanceReportQuery { StudentId = student, StartDate = Request.StartDate });
                reports.Add(report);
            }

            // Zip all reports
            using var memoryStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create))
            {
                foreach (var file in reports)
                {
                    var zipArchiveEntry = zipArchive.CreateEntry(file.Name);
                    using var streamWriter = new StreamWriter(zipArchiveEntry.Open());
                    var fileData = file.FileData;
                    streamWriter.BaseStream.Write(fileData, 0, fileData.Length);
                }
            }

            var attachmentStream = new MemoryStream(memoryStream.ToArray());

            return File(attachmentStream.ToArray(), MediaTypeNames.Application.Zip, "Attendance Reports.zip");
        }
        else
        {
            // We only have one student, so just download that file.
            var file = await _mediator.Send(new GetStudentAttendanceReportQuery { StudentId = Request.Students.First(), StartDate = Request.StartDate });

            return File(file.FileData, file.FileType, file.Name);
        }
    }
}
