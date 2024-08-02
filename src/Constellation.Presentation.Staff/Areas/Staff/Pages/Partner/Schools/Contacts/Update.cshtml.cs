namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools.Contacts;

using Application.Common.PresentationModels;
using Constellation.Application.Helpers;
using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.GetContactSummary;
using Constellation.Application.SchoolContacts.UpdateContact;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.ModelBinders;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanManageSchoolContacts)]
public class UpdateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpdateModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpdateModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Contacts;
    [ViewData] public string PageTitle => "Update School Contact";


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
    public string? PhoneNumber { get; set; } = string.Empty;

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve School Contact with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

        Result<ContactSummaryResponse> contact = await _mediator.Send(new GetContactSummaryQuery(Id));

        if (contact.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                contact.Error,
                _linkGenerator.GetPathByPage("/Partner/Schools/Contacts/Index", values: new { area = "Staff" }));

            _logger
                .ForContext(nameof(Error), contact.Error, true)
                .Warning("Failed to retrieve School Contact with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

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

        UpdateContactCommand command = new(
            Id,
            FirstName,
            LastName,
            EmailAddress,
            PhoneNumber);

        _logger
            .ForContext(nameof(UpdateContactCommand), command, true)
            .Information("Requested to update School Contact with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            ModalContent = new ErrorDisplay(request.Error);

            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to update School Contact with id {Id} by user {User}", Id, _currentUserService.UserName);

            return Page();
        }

        return RedirectToPage("/Partner/Schools/Contacts/Index", new { area = "Staff" });
    }
}