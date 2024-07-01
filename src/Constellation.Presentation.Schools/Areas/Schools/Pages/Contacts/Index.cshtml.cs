namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Contacts;

using Application.Models.Auth;
using Application.SchoolContacts.GetContactsWithRoleFromSchool;
using Constellation.Application.SchoolContacts.RequestContactRemoval;
using Constellation.Application.Schools.GetSchoolContactDetails;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Schools.Pages.Shared.PartialViews.RemoveContactConfirmation;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Contacts;

    public SchoolContactDetailsResponse SchoolDetails { get; set; }
    public List<ContactResponse> Contacts { get; set; } = new();

    public string Message { get; set; } = string.Empty;

    public async Task OnGet()
    {
        Result<SchoolContactDetailsResponse> schoolDetailsRequest = await _mediator.Send(new GetSchoolContactDetailsQuery(CurrentSchoolCode));

        if (schoolDetailsRequest.IsFailure)
        {
            Error = new()
            {
                Error = schoolDetailsRequest.Error,
                RedirectPath = null
            };

            return;
        }

        SchoolDetails = schoolDetailsRequest.Value;

        Result<List<ContactResponse>> contactsRequest = await _mediator.Send(new GetContactsWithRoleFromSchoolQuery(CurrentSchoolCode));
        
        if (contactsRequest.IsFailure)
        {
            Error = new()
            {
                Error = contactsRequest.Error,
                RedirectPath = null
            };

            return;
        }

        Contacts = contactsRequest.Value
            .OrderBy(contact => contact.PositionSort())
            .ThenBy(contact => contact.LastName)
            .ToList();
    }

    public async Task<IActionResult> OnPostAjaxRemoveContact(Guid assignmentId)
    {
        SchoolContactRoleId roleId = SchoolContactRoleId.FromValue(assignmentId);

        Result<List<ContactResponse>> contactsRequest = await _mediator.Send(new GetContactsWithRoleFromSchoolQuery(CurrentSchoolCode));

        ContactResponse? contact = contactsRequest.Value.FirstOrDefault(entry => entry.AssignmentId == roleId);

        RemoveContactConfirmationViewModel viewModel = new()
        {
            Contact = contact,
            ContactId = contact.ContactId,
            AssignmentId = contact.AssignmentId
        };

        return Partial("RemoveContactConfirmation", viewModel);
    }

    public async Task<IActionResult> OnPostRemoveContact(RemoveContactConfirmationViewModel viewModel)
    {
        RequestContactRemovalCommand command = new(
            viewModel.ContactId,
            viewModel.AssignmentId,
            viewModel.Comment,
            _currentUserService.UserName,
            string.Empty);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            return Page();
        }

        Message = "A request has been sent to Aurora College for this contact to be removed.";

        Result<SchoolContactDetailsResponse> schoolDetailsRequest = await _mediator.Send(new GetSchoolContactDetailsQuery(CurrentSchoolCode));

        SchoolDetails = schoolDetailsRequest.Value;

        Result<List<ContactResponse>> contactsRequest = await _mediator.Send(new GetContactsWithRoleFromSchoolQuery(CurrentSchoolCode));

        Contacts = contactsRequest.Value
            .OrderBy(contact => contact.PositionSort())
            .ThenBy(contact => contact.LastName)
            .ToList();

        return Page();
    }
}