namespace Constellation.Portal.Schools.Server.Controllers;

using Application.Models.Identity;
using Application.Students.GetCurrentStudentsFromSchool;
using Constellation.Application.DTOs;
using Constellation.Application.Students.GetStudentsFromSchoolForSelectionList;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
public class StudentsController : BaseAPIController
{
    private readonly IMediator _mediator;
    private readonly Serilog.ILogger _logger;

    public StudentsController(IMediator mediator, Serilog.ILogger logger)
    {
        _mediator = mediator;
        _logger = logger.ForContext<StudentsController>();
    }

    [HttpGet("FromSchool/{schoolCode}")]
    public async Task<ApiResult<List<StudentDto>>> GetForSchool(string schoolCode)
    {
        Result<List<StudentDto>>? students = await _mediator.Send(new GetCurrentStudentsFromSchoolQuery(schoolCode));

        return ApiResult.FromResult(students);
    }

    [HttpGet("ForDropdown/{schoolCode}")]
    public async Task<ApiResult<List<StudentSelectionResponse>>> GetStudentsForDropdown([FromRoute] string schoolCode)
    {
        AppUser? user = await GetCurrentUser();

        _logger.Information("Requested to retrieve students for school {schoolCode} by user {user}", schoolCode, user.DisplayName);

        Result<List<StudentSelectionResponse>>? students = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery(schoolCode));

        return ApiResult.FromResult(students);
    }
}
