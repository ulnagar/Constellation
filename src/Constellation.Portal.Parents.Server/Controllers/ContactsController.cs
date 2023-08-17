namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.Contacts.GetContactListForParentPortal;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
public class ContactsController : BaseAPIController
{
	private readonly ILogger<ContactsController> _logger;
	private readonly IMediator _mediator;

	public ContactsController(ILogger<ContactsController> logger, IMediator mediator)
	{
		_logger = logger;
		_mediator = mediator;
	}

	[HttpGet("All/{studentId}")]
	public async Task<List<StudentSupportContactResponse>> GetAllContacts([FromRoute] string studentId)
	{
		var authorised = await HasAuthorizedAccessToStudent(_mediator, studentId);

		if (!authorised)
		{
			return new List<StudentSupportContactResponse>();
		}

		var user = await GetCurrentUser();

		_logger.LogInformation("Requested to retrieve contacts for student {studentId} by user {user}", studentId, user.UserName);

		var request = await _mediator.Send(new GetContactListForParentPortalQuery(studentId));

        if (request.IsFailure)
        {
            return new List<StudentSupportContactResponse>();
        }

        return request.Value;
	}
}
