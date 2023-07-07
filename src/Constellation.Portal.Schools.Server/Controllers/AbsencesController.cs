namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.Absences.CreateAbsenceResponseFromSchool;
using Constellation.Application.Absences.GetAbsenceDetailsForSchool;
using Constellation.Application.Absences.GetAbsenceResponseDetailsForSchool;
using Constellation.Application.Absences.GetOutstandingAbsencesForSchool;
using Constellation.Application.Absences.RejectStudentExplanation;
using Constellation.Application.Absences.VerifyStudenExplanation;
using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Core.Models;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Portal.Schools.Client.Shared.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Net.Mime;

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
    public async Task<List<OutstandingAbsencesForSchoolResponse>> GetForSchool(string schoolCode, CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absences for school {code} by user {user}", schoolCode, user.DisplayName);

        Result<List<OutstandingAbsencesForSchoolResponse>> request = await _mediator.Send(new GetOutstandingAbsencesForSchoolQuery(schoolCode), cancellationToken);

        if (request.IsFailure)
        {
            _logger.Warning("Could not retrieve absences for school {code} due to error {@error}", schoolCode, request.Error);
            return new List<OutstandingAbsencesForSchoolResponse>();
        }

        return request.Value;
    }

    [HttpGet("Whole/{absenceId:guid}")]
    public async Task<SchoolAbsenceDetailsResponse?> GetWholeAbsenceForExplanation(Guid absenceId, CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absence for explanation by user {user} with id {id}", user.DisplayName, absenceId.ToString());

        AbsenceId Id = AbsenceId.FromValue(absenceId);

        Result<SchoolAbsenceDetailsResponse> request = await _mediator.Send(new GetAbsenceDetailsForSchoolQuery(Id), cancellationToken);

        if (request.IsFailure)
        {
            return null;
        }

        return request.Value;
    }

    [HttpPost("Whole/{absenceId:guid}/Explain")]
    public async Task ExplainWholeAbsence([FromQuery] Guid AbsenceId, [FromBody] CreateAbsenceResponseFromSchoolCommand Command)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to explain absence by user {user} with details {@absence}", user.DisplayName, Command);

        await _mediator.Send(Command);
    }

    [HttpGet("Partial/{absenceId:guid}/Response/{responseId:guid}")]
    public async Task<SchoolAbsenceResponseDetailsResponse?> GetPartialAbsenceForVerification(Guid absenceId, Guid responseId, CancellationToken cancellationToken = default)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absence for verification by user {user} with id {id}", user.DisplayName, responseId.ToString());

        AbsenceId AbsenceId = AbsenceId.FromValue(absenceId);
        AbsenceResponseId ResponseId = AbsenceResponseId.FromValue(responseId);

        Result<SchoolAbsenceResponseDetailsResponse> request = await _mediator.Send(new GetAbsenceResponseDetailsForSchoolQuery(AbsenceId, ResponseId), cancellationToken);

        if (request.IsFailure)
            return null;

        return request.Value;
    }

    [HttpPost("Partial/{responseId:guid}/Verify")]
    public async Task VerifyPartialAbsence([FromQuery] Guid ResponseId, [FromBody] VerifyStudentExplanationCommand Command)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to verify absence explanation by user {user} with details {@absence}", user.DisplayName, Command);

        await _mediator.Send(Command);
    }

    [HttpPost("Partial/{responseId:guid}/Reject")]
    public async Task RejectPartialAbsence([FromQuery] Guid ResponseId, [FromBody] RejectStudentExplanationCommand Command)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to reject absence explanation by user {user} with details {@absence}", user.DisplayName, Command);

        await _mediator.Send(Command);
    }

    [HttpPost("Report")]
    public async Task<IActionResult> GenerateAttendanceReports([FromBody] AttendanceReportSelectForm Request, CancellationToken cancellationToken = default)
    {
        var endDate = Request.StartDate.AddDays(12);

        if (Request.Students.Count > 1)
        {
            List<StoredFile> reports = new();

            // We need to loop each student id and collate the report into a zip file.
            foreach (string studentId in Request.Students)
            {
                Result<StoredFile> reportRequest = await _mediator.Send(new GenerateAttendanceReportForStudentQuery(studentId, Request.StartDate, endDate), cancellationToken);
                
                if (reportRequest.IsSuccess)
                    reports.Add(reportRequest.Value);
            }

            // Zip all reports
            using MemoryStream memoryStream = new MemoryStream();
            using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create))
            {
                foreach (StoredFile file in reports)
                {
                    ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(file.Name);
                    using StreamWriter streamWriter = new StreamWriter(zipArchiveEntry.Open());
                    byte[] fileData = file.FileData;
                    streamWriter.BaseStream.Write(fileData, 0, fileData.Length);
                }
            }

            MemoryStream attachmentStream = new MemoryStream(memoryStream.ToArray());

            return File(attachmentStream.ToArray(), MediaTypeNames.Application.Zip, "Attendance Reports.zip");
        }
        else
        {
            // We only have one student, so just download that file.
            Result<StoredFile> fileRequest = await _mediator.Send(new GenerateAttendanceReportForStudentQuery(Request.Students.First(), Request.StartDate, endDate), cancellationToken);

            if (fileRequest.IsFailure)
                return BadRequest();

            return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.Name);
        }
    }
}
