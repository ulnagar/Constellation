namespace Constellation.Portal.Schools.Server.Controllers;

using Constellation.Application.DTOs;
using Constellation.Application.Features.Portal.School.Home.Models;
using Constellation.Application.Features.Portal.School.Home.Queries;
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
    public async Task<List<StudentDto>> GetForSchool(string schoolCode)
    {
        var students = await _mediator.Send(new GetStudentsFromSchoolQuery { SchoolCode = schoolCode });

        return students.ToList();
    }

    [HttpGet("ForDropdown/{schoolCode}")]
    public async Task<List<StudentFromSchoolForDropdownSelection>> GetStudentsForDropdown([FromRoute] string schoolCode)
    {
        var user = await GetCurrentUser();

        _logger.Information("Requested to retrieve students for school {schoolCode} by user {user}", schoolCode, user.DisplayName);

        var students = await _mediator.Send(new GetStudentsFromSchoolForSelectionQuery { SchoolCode = schoolCode });

        return students.ToList();
    }
}
