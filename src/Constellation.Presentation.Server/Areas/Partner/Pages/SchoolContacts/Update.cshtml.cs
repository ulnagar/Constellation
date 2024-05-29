namespace Constellation.Presentation.Server.Areas.Partner.Pages.SchoolContacts;

using Application.Models.Auth;
using Application.SchoolContacts.GetContactSummary;
using Application.SchoolContacts.UpdateContact;
using BaseModels;
using Constellation.Application.Helpers;
using Core.Models.SchoolContacts.Identifiers;
using Core.Shared;
using Helpers.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Helpers.ModelBinders;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanManageSchoolContacts)]
public class UpdateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpdateModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => SchoolContactsPages.Contacts;

    [ModelBinder(typeof(StrongIdBinder))]
    [BindProperty(SupportsGet = true)]
    public SchoolContactId Id { get; set; }

    [BindProperty]
    [Required]
    [Display(Name = DisplayNameDefaults.FirstName)]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [Display(Name = DisplayNameDefaults.LastName)]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    [Required]
    [DataType(DataType.EmailAddress)]
    [Display(Name = DisplayNameDefaults.EmailAddress)]
    public string EmailAddress { get; set; } = string.Empty;

    [BindProperty]
    [DataType(DataType.PhoneNumber)]
    [Display(Name = DisplayNameDefaults.PhoneNumber)]
    public string PhoneNumber { get; set; } = string.Empty;

    public async Task OnGet()
    {
        Result<ContactSummaryResponse> contact = await _mediator.Send(new GetContactSummaryQuery(Id));

        if (contact.IsFailure)
        {
            Error = new()
            {
                Error = contact.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolContacts/Index", values: new { area = "Partner" })
            };

            return;
        }

        FirstName = contact.Value.FirstName;
        LastName = contact.Value.LastName;
        EmailAddress = contact.Value.EmailAddress;
        PhoneNumber = contact.Value.PhoneNumber;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        FirstName = FirstName.Trim();
        LastName = LastName.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? PhoneNumber : PhoneNumber.Trim();
        EmailAddress = EmailAddress.Trim();

        Result request = await _mediator.Send(new UpdateContactCommand(
            Id,
            FirstName,
            LastName,
            EmailAddress,
            PhoneNumber));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage("/SchoolContacts/Index", new { area = "Partner" });
    }
}