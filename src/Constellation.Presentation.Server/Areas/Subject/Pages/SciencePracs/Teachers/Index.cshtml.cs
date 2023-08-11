namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Teachers;

using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.GetAllSciencePracTeachers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData]
    public string ActivePage => "Teachers";

    public List<ContactResponse> Contacts = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Result<List<ContactResponse>> contactRequest = await _mediator.Send(new GetAllSciencePracTeachersQuery());

        if (contactRequest.IsFailure)
        {
            Error = new()
            {
                Error = contactRequest.Error,
                RedirectPath = null
            };

            return;
        }

        Contacts = contactRequest.Value.OrderBy(contact => contact.ContactName.SortOrder).ToList();
    }
}
