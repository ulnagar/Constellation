namespace Constellation.Portal.Schools.Server.Controllers;

using Application.DTOs;
using Application.Models.Identity;
using Constellation.Application.Absences.CreateAbsenceResponseFromSchool;
using Constellation.Application.Absences.GetAbsenceDetailsForSchool;
using Constellation.Application.Absences.GetAbsenceResponseDetailsForSchool;
using Constellation.Application.Absences.GetOutstandingAbsencesForSchool;
using Constellation.Application.Absences.RejectStudentExplanation;
using Constellation.Application.Absences.VerifyStudenExplanation;
using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Portal.Schools.Client.Shared.Models;
using Core.Errors;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Net.Mime;

[Route("api/[controller]")]
public class AbsencesController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public AbsencesController(
        IMediator mediator, 
        Serilog.ILogger logger)
    {
        _mediator = mediator;
        _logger = logger.ForContext<AbsencesController>();
    }

    [HttpGet("{schoolCode}/All")]
    public async Task<ApiResult<List<OutstandingAbsencesForSchoolResponse>>> GetForSchool(string schoolCode, CancellationToken cancellationToken = default)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absences for school {code} by user {user}", schoolCode, user.DisplayName);

        Result<List<OutstandingAbsencesForSchoolResponse>> request = await _mediator.Send(new GetOutstandingAbsencesForSchoolQuery(schoolCode), cancellationToken);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(schoolCode), schoolCode)
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Could not retrieve absences for school {code} due to error {@error}", schoolCode, request.Error);
        }

        return ApiResult.FromResult(request);
    }

    [HttpGet("Whole/{absenceId:guid}")]
    public async Task<ApiResult<SchoolAbsenceDetailsResponse>> GetWholeAbsenceForExplanation(Guid absenceId, CancellationToken cancellationToken = default)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absence for explanation by user {user} with id {id}", user.DisplayName, absenceId.ToString());

        AbsenceId id = AbsenceId.FromValue(absenceId);

        Result<SchoolAbsenceDetailsResponse> request = await _mediator.Send(new GetAbsenceDetailsForSchoolQuery(id), cancellationToken);

        return ApiResult.FromResult(request);
    }

    [HttpPost("Whole/{absenceId:guid}/Explain")]
    public async Task<ApiResult> ExplainWholeAbsence(Guid absenceId, AbsencesExplainFormModel Form)
    {
        AppUser? user = await GetCurrentUser();

        if (absenceId != Form.AbsenceId)
        {
            // Something is wrong here!
            _logger
                .ForContext("Route AbsenceId", absenceId)
                .ForContext(nameof(AbsencesExplainFormModel), Form, true)
                .Warning("Route AbsenceId does not match Form AbsenceId");

            return ApiResult.FromResult(Result.Failure(
                new(
                    "Application.Error",
                    "Routing Error: Route does not match supplied form")));
        }

        AbsenceId AbsenceId = AbsenceId.FromValue(Form.AbsenceId);

        CreateAbsenceResponseFromSchoolCommand Command = new(
            AbsenceId,
            Form.Comment,
            user.Email);

        _logger.Information("Requested to explain absence by user {user} with details {@absence}", user.DisplayName, Command);

        Result result = await _mediator.Send(Command);

        return ApiResult.FromResult(result);
    }

    [HttpGet("Partial/{absenceId:guid}/Response/{responseId:guid}")]
    public async Task<ApiResult<SchoolAbsenceResponseDetailsResponse>> GetPartialAbsenceForVerification(Guid absenceId, Guid responseId, CancellationToken cancellationToken = default)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absence for verification by user {user} with id {id}", user.DisplayName, responseId.ToString());

        AbsenceId AbsenceId = AbsenceId.FromValue(absenceId);
        AbsenceResponseId ResponseId = AbsenceResponseId.FromValue(responseId);

        Result<SchoolAbsenceResponseDetailsResponse> request = await _mediator.Send(new GetAbsenceResponseDetailsForSchoolQuery(AbsenceId, ResponseId), cancellationToken);

        return ApiResult.FromResult(request);
    }

    [HttpPost("Partial/{absenceId:guid}/Response/{responseId:guid}/Verify")]
    public async Task<ApiResult> VerifyPartialAbsence(Guid absenceId, Guid responseId, AbsencesVerifyFormModel Form)
    {
        AppUser? user = await GetCurrentUser();

        if (absenceId != Form.AbsenceId)
        {
            // Something is wrong here!
            _logger
                .ForContext("Route AbsenceId", absenceId)
                .ForContext(nameof(AbsencesVerifyFormModel), Form, true)
                .Warning("Route AbsenceId does not match Form AbsenceId");

            return ApiResult.FromResult(Result.Failure(
                new(
                    "Application.Error",
                    "Routing Error: Route does not match supplied form")));
        }

        if (responseId != Form.ResponseId)
        {
            // Something is wrong here!
            _logger
                .ForContext("Route ResponseId", responseId)
                .ForContext(nameof(AbsencesVerifyFormModel), Form, true)
                .Warning("Route ResponseId does not match Form ResponseId");

            return ApiResult.FromResult(Result.Failure(
                new(
                    "Application.Error",
                    "Routing Error: Route does not match supplied form")));
        }

        AbsenceId AbsenceId = AbsenceId.FromValue(absenceId);
        AbsenceResponseId ResponseId = AbsenceResponseId.FromValue(responseId);

        VerifyStudentExplanationCommand command = new(
            AbsenceId,
            ResponseId,
            Form.Username ?? user.Email,
            Form.Comment);

        _logger.Information("Requested to verify absence explanation by user {user} with details {@absence}", user.DisplayName, command);

        Result response = await _mediator.Send(command);

        return ApiResult.FromResult(response);
    }

    [HttpPost("Partial/{absenceId:guid}/Response/{responseId:guid}/Reject")]
    public async Task<ApiResult> RejectPartialAbsence(Guid absenceId, Guid responseId, AbsencesVerifyFormModel Form)
    {
        AppUser? user = await GetCurrentUser();

        if (absenceId != Form.AbsenceId)
        {
            // Something is wrong here!
            _logger
                .ForContext("Route AbsenceId", absenceId)
                .ForContext(nameof(AbsencesVerifyFormModel), Form, true)
                .Warning("Route AbsenceId does not match Form AbsenceId");

            return ApiResult.FromResult(Result.Failure(
                new(
                    "Application.Error",
                    "Routing Error: Route does not match supplied form")));
        }

        if (responseId != Form.ResponseId)
        {
            // Something is wrong here!
            _logger
                .ForContext("Route ResponseId", responseId)
                .ForContext(nameof(AbsencesVerifyFormModel), Form, true)
                .Warning("Route ResponseId does not match Form ResponseId");

            return ApiResult.FromResult(Result.Failure(
                new(
                    "Application.Error",
                    "Routing Error: Route does not match supplied form")));
        }

        AbsenceId AbsenceId = AbsenceId.FromValue(absenceId);
        AbsenceResponseId ResponseId = AbsenceResponseId.FromValue(responseId);

        RejectStudentExplanationCommand command = new(
            AbsenceId,
            ResponseId,
            Form.Username ?? user.Email,
            Form.Comment);

        _logger.Information("Requested to reject absence explanation by user {user} with details {@absence}", user.DisplayName, command);

        Result response = await _mediator.Send(command);

        return ApiResult.FromResult(response);
    }

    [HttpPost("Report")]
    public async Task<IActionResult> GenerateAttendanceReports([FromBody] AttendanceReportSelectForm Request, CancellationToken cancellationToken = default)
    {
        DateOnly startDate = DateOnly.FromDateTime(Request.StartDate);
        DateOnly endDate = startDate.AddDays(12);

        if (Request.Students.Count > 1)
        {
            List<FileDto> reports = new();

            // We need to loop each student id and collate the report into a zip file.
            foreach (string studentId in Request.Students)
            {
                Result<FileDto> reportRequest = await _mediator.Send(new GenerateAttendanceReportForStudentQuery(studentId, startDate, endDate), cancellationToken);

                if (reportRequest.IsFailure)
                    return Ok(ApiResult.FromResult(reportRequest));

                if (reportRequest.IsSuccess)
                    reports.Add(reportRequest.Value);
            }

            // Zip all reports
            using MemoryStream memoryStream = new();
            using (ZipArchive zipArchive = new(memoryStream, ZipArchiveMode.Create))
            {
                foreach (FileDto file in reports)
                {
                    ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(file.FileName);
                    await using StreamWriter streamWriter = new(zipArchiveEntry.Open());
                    byte[] fileData = file.FileData;
                    await streamWriter.BaseStream.WriteAsync(fileData, 0, fileData.Length, cancellationToken);
                }
            }

            MemoryStream attachmentStream = new(memoryStream.ToArray());

            return File(attachmentStream.ToArray(), MediaTypeNames.Application.Zip, "Attendance Reports.zip");
        }

        // We only have one student, so just download that file.
        Result<FileDto> fileRequest = await _mediator.Send(new GenerateAttendanceReportForStudentQuery(Request.Students.First(), startDate, endDate), cancellationToken);

        if (fileRequest.IsFailure)
            return Ok(ApiResult.FromResult(fileRequest));

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
}
