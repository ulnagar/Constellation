namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.DTOs;
using Constellation.Application.Features.Portal.School.Home.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ILogger<StudentsController> _logger;
    private readonly IMediator _mediator;

    public StudentsController(ILogger<StudentsController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ICollection<StudentDto>> Get()
    {
        _logger.LogInformation("Started getting students!");

        return await _mediator.Send(new GetStudentsFromSchoolQuery { SchoolCode = "8155" });
    }
}