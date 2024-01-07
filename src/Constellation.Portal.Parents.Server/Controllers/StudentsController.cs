namespace Constellation.Portal.Parents.Server.Controllers;

using Application.Models.Identity;
using Constellation.Application.Students.GetStudentsByParentEmail;
using Core.Shared;
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
    public async Task<ApiResult<List<StudentResponse>>> Get()
    {
        AppUser? user = await GetCurrentUser();

        Result<List<StudentResponse>>? request = await _mediator.Send(new GetStudentsByParentEmailQuery(user.Email));
        
        return ApiResult.FromResult(request);
    }
}