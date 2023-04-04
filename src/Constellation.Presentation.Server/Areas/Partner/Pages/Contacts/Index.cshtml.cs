namespace Constellation.Presentation.Server.Areas.Partner.Pages.Contacts;

using Constellation.Application.Contacts.GetContactList;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public FilterDefinition Filter { get; set; } = new();

    public List<ContactResponse> Contacts { get; set; } = new();

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken)
    {
        await GetClasses(_mediator);

        var contactRequest = await _mediator.Send(
            new GetContactListQuery(
                Filter.Offerings,
                Filter.Grades,
                Filter.Schools,
                Filter.Categories),
            cancellationToken);

        if (contactRequest.IsFailure)
        {
            // uh oh!
        }

        Contacts = contactRequest.Value;

        Contacts = Contacts
            .OrderBy(contact => contact.StudentGrade)
            .ThenBy(contact => contact.Student.LastName)
            .ThenBy(contact => contact.Student.FirstName)
            .ToList();

        return Page();
    }
    public async Task<IActionResult> OnPostFilter(CancellationToken cancellationToken)
    {
        await GetClasses(_mediator);

        var contactRequest = await _mediator.Send(
            new GetContactListQuery(
                Filter.Offerings,
                Filter.Grades,
                Filter.Schools,
                Filter.Categories),
            cancellationToken);

        if (contactRequest.IsFailure)
        {
            // uh oh!
        }

        Contacts = contactRequest.Value;

        Contacts = Contacts
            .OrderBy(contact => contact.StudentGrade)
            .ThenBy(contact => contact.Student.LastName)
            .ThenBy(contact => contact.Student.FirstName)
            .ToList();

        return Page();
    }


    public class FilterDefinition
    {
        public List<int> Offerings { get; set; } = new();
        public List<Grade> Grades { get; set; } = new();
        public List<string> Schools { get; set; } = new();
        public List<ContactCategory> Categories { get; set; } = new();
    }
}
