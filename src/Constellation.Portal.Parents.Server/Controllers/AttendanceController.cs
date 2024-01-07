namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.Absences.GetAbsenceDetailsForParent;
using Constellation.Application.Absences.GetAbsencesForFamily;
using Constellation.Application.Absences.ProvideParentWholeAbsenceExplanation;
using Constellation.Application.Attendance.GenerateAttendanceReportForStudent;
using Constellation.Application.Attendance.GetValidAttendanceReportDates;
using Constellation.Application.DTOs;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Core.Errors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class AttendanceController : BaseAPIController
{
    private readonly Serilog.ILogger _logger;
    private readonly IMediator _mediator;

    public AttendanceController(
        Serilog.ILogger logger, 
        IMediator mediator)
    {
        _logger = logger.ForContext<AttendanceController>();
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ApiResult<List<AbsenceForFamilyResponse>>> Get()
    {
        AppUser user = await GetCurrentUser();

        _logger.Information("Requested to retrieve attendance data for parent {name}", user.UserName);

        Result<List<AbsenceForFamilyResponse>>? request = await _mediator.Send(new GetAbsencesForFamilyQuery(user.Email));

        return ApiResult.FromResult(request);
    }

    [HttpGet("Details/{id:guid}")]
    public async Task<ApiResult<ParentAbsenceDetailsResponse>> GetDetails([FromRoute] Guid Id)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absence details for id {id} by parent {name}", Id, user.UserName);

        // This Mediator Handler is secured so that data is only returned if the parent email matches the absence id.
        AbsenceId? absenceId = AbsenceId.FromValue(Id);

        Result<ParentAbsenceDetailsResponse>? response = await _mediator.Send(new GetAbsenceDetailsForParentQuery(user.Email, absenceId));

        if (response.IsFailure)
            _logger.Information("Failed to retrieve absence details for id {id} by parent {name} with error {@error}", Id, user.UserName, response.Error);

        return ApiResult.FromResult(response);
    }

    [HttpPost("ParentExplanation")]
    public async Task<ApiResult> Explain([FromBody] ProvideParentWholeAbsenceExplanationCommand command)
    {
        AppUser? user = await GetCurrentUser();

        _logger
            .ForContext(nameof(ProvideParentWholeAbsenceExplanationCommand), command, true)
            .Information("Requested to record explanation of absence for id {id} by parent {name}", command.AbsenceId, user.UserName);

        command.ParentEmail = user.Email;

        // This Mediator Handler is secured so that the data is only saved if the parent email matches the absence id.
        Result? response = await _mediator.Send(command);

        return ApiResult.FromResult(response);
    }

    [HttpGet("Reports/Dates")]
    public async Task<ApiResult<List<ValidAttendenceReportDate>>> GetReportDates()
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve absence report dates by parent {name}", user.UserName);

        Result<List<ValidAttendenceReportDate>> request = await _mediator.Send(new GetValidAttendenceReportDatesQuery());

        return ApiResult.FromResult(request);
    }

    [HttpPost("Reports/Download")]
    public async Task<IActionResult> GetAttendanceReport([FromBody] AttendanceReportRequest request, CancellationToken cancellationToken = default)
    {
        AppUser? user = await GetCurrentUser();

        _logger
            .ForContext(nameof(AttendanceReportRequest), request, true)
            .Information("Requested to retrieve attendance report by parent {name}", user.UserName);


        bool authorised = await HasAuthorizedAccessToStudent(_mediator, request.StudentId);

        if (!authorised)
        {
            _logger
                .ForContext(nameof(AttendanceReportRequest), request, true)
                .ForContext(nameof(AppUser), user, true)
                .Warning("Unauthorised attempt to download attendance report by parent {name}", user.UserName);

            return Ok(ApiResult.FromResult(Result.Failure(DomainErrors.Auth.NotAuthorised)));
        }

        // Create file as stream
        GenerateAttendanceReportForStudentQuery attendanceReportRequest = new(
            request.StudentId,
            DateOnly.FromDateTime(request.StartDate), 
            DateOnly.FromDateTime(request.EndDate));

        Result<FileDto> fileRequest = await _mediator.Send(attendanceReportRequest, cancellationToken);

        if (fileRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(GenerateAttendanceReportForStudentQuery), attendanceReportRequest, true)
                .ForContext(nameof(AttendanceReportRequest), request, true)
                .ForContext(nameof(AppUser), user, true)
                .ForContext(nameof(Error), fileRequest.Error, true)
                .Warning("Failed attempt to download attendance report by parent {name}", user.UserName);

            return Ok(ApiResult.FromResult(Result.Failure(fileRequest.Error)));
        }

        return File(fileRequest.Value.FileData, fileRequest.Value.FileType, fileRequest.Value.FileName);
    }
}