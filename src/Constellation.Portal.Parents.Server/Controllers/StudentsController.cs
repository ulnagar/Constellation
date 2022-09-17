namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.Features.Partners.Students.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class StudentsController : BaseAPIController
{
    private readonly ILogger<StudentsController> _logger;
    private readonly IMediator _mediator;

    public StudentsController(ILogger<StudentsController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ICollection<StudentOfParent>> Get()
    {
        var user = await GetCurrentUser();

        return await _mediator.Send(new GetStudentsOfParentQuery { ParentEmail = user.Email });
    }
}