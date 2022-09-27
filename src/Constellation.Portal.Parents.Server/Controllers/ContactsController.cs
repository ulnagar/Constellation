namespace Constellation.Portal.Parents.Server.Controllers;

using Constellation.Application.Features.Contacts.Queries;
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
	public async Task<ICollection<StudentSupportContact>> GetAllContacts([FromRoute] string studentId)
	{
		//var user = await GetCurrentUser();

		//_logger.LogInformation("Requested to retrieve contacts for student {studentId} by user {user}", studentId, user.UserName);

		return await _mediator.Send(new GetStudentSupportContactsQuery { StudentId = studentId });
	}
}
