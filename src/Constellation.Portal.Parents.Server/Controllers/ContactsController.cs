namespace Constellation.Portal.Parents.Server.Controllers;

using Application.Models.Identity;
using Constellation.Application.Contacts.GetContactListForParentPortal;
using Core.Errors;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class ContactsController : BaseAPIController
{
	private readonly Serilog.ILogger _logger;
	private readonly IMediator _mediator;

	public ContactsController(
        Serilog.ILogger logger, 
        IMediator mediator)
	{
		_logger = logger.ForContext<ContactsController>();
		_mediator = mediator;
	}

	[HttpGet("All/{studentId}")]
	public async Task<ApiResult<List<StudentSupportContactResponse>>> GetAllContacts([FromRoute] string studentId)
	{
		bool authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

		if (!authorised)
			return ApiResult.FromResult(Result.Failure<List<StudentSupportContactResponse>>(DomainErrors.Auth.NotAuthorised));

		AppUser? user = await GetCurrentUser();

		_logger.Information("Requested to retrieve contacts for student {studentId} by user {user}", studentId, user.UserName);

		Result<List<StudentSupportContactResponse>>? request = await _mediator.Send(new GetContactListForParentPortalQuery(studentId));

        return ApiResult.FromResult(request);
    }
}
