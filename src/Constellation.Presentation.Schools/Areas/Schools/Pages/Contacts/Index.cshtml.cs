namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Contacts;

using Application.Common.PresentationModels;
using Application.Domains.SchoolContacts.Commands.RequestContactRemoval;
using Application.Domains.SchoolContacts.Queries.GetContactsWithRoleFromSchool;
using Application.Domains.Schools.Queries.GetSchoolContactDetails;
using Application.Models.Auth;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Schools.Pages.Shared.PartialViews.RemoveContactConfirmation;
using Serilog;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.Contacts;

    public SchoolContactDetailsResponse SchoolDetails { get; set; }
    public List<ContactResponse> Contacts { get; set; } = new();

    public string Message { get; set; } = string.Empty;

    public async Task OnGet() => await PreparePage();

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve school data by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

        Result<SchoolContactDetailsResponse> schoolDetailsRequest = await _mediator.Send(new GetSchoolContactDetailsQuery(CurrentSchoolCode));

        if (schoolDetailsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(schoolDetailsRequest.Error);

            return;
        }

        SchoolDetails = schoolDetailsRequest.Value;

        _logger.Information("Requested to retrieve contact list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

        Result<List<ContactResponse>> contactsRequest = await _mediator.Send(new GetContactsWithRoleFromSchoolQuery(CurrentSchoolCode, true));

        if (contactsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(contactsRequest.Error);

            return;
        }

        Contacts = contactsRequest.Value
            .OrderBy(contact => contact.Position.SortOrder)
            .ThenBy(contact => contact.LastName)
            .ToList();
    }

    public async Task<IActionResult> OnPostAjaxRemoveContact(Guid assignmentId)
    {
        SchoolContactRoleId roleId = SchoolContactRoleId.FromValue(assignmentId);

        Result<List<ContactResponse>> contactsRequest = await _mediator.Send(new GetContactsWithRoleFromSchoolQuery(CurrentSchoolCode, true));

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

        _logger.Information("Requested to remove school contact by user {user} with record {@command}", _currentUserService.UserName, command);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(result.Error);

            return Page();
        }

        ModalContent = FeedbackDisplay.Create(
            "Schools Portal",
            "A request has been sent to Aurora College for this contact to be removed.",
            "Ok",
            "btn-success");

        await PreparePage();

        return Page();
    }
}