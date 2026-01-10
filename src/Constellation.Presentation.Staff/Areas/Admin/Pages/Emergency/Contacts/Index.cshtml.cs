namespace Constellation.Presentation.Staff.Areas.Admin.Pages.Emergency.Contacts;

using Application.Common.PresentationModels;
using Application.Domains.EmergencyConsole.Queries.GetContactDetails;
using Application.Domains.SchoolContacts.Commands.UpdateSchoolContactPhoneNumber;
using Application.Domains.SchoolContacts.Queries.GetContactSummary;
using Application.Domains.StaffMembers.Commands.UpdateStaffMemberPhoneNumber;
using Application.Domains.StaffMembers.Queries.GetStaffById;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Core.Models.SchoolContacts.Identifiers;
using Core.Models.StaffMembers.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;
using Shared.PartialViews.AddPhoneNumberToSchoolContact;
using Shared.PartialViews.AddPhoneNumberToStaffMember;

[HasPermission(AuthPermission.Admin_EmergencyConsole_Edit_Value)]
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
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>();
    }

    [ViewData]
    public string ActivePage => Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Admin_Emergency_Contacts;

    [ViewData]
    public string PageTitle => "Contacts";

    public List<ContactDetail> Contacts { get; set; } = [];

    public async Task OnGet()
    {
        Result<List<ContactDetail>> contacts = await _mediator.Send(new GetContactDetailsQuery());

        if (contacts.IsFailure)
        {
            return;
        }

        Contacts = contacts.Value;
    }

    public async Task<IActionResult> OnPostAjaxStaffPhoneUpdate(StaffId staffId)
    {
        Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(staffId));

        if (staffMember.IsFailure)
            return Content(string.Empty);

        AddPhoneNumberToStaffMemberViewModel viewModel = new()
        {
            StaffId = staffMember.Value.StaffId,
            PhoneNumber = staffMember.Value.PhoneNumber
        };

        return Partial("AddPhoneNumberToStaffMember", viewModel);
    }

    public async Task<IActionResult> OnPostStaffPhoneUpdate(AddPhoneNumberToStaffMemberViewModel viewModel)
    {
        if (viewModel.PhoneNumber == PhoneNumber.Empty)
            return RedirectToPage();

        Result update = await _mediator.Send(new UpdateStaffMemberPhoneNumberCommand(viewModel.StaffId, viewModel.PhoneNumber));

        if (update.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                update.Error,
                _linkGenerator.GetPathByPage("/Emergency/Contacts/Index", values: new { area = "Admin" }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAjaxContactPhoneUpdate(SchoolContactId contactId)
    {
        Result<ContactSummaryResponse> contact = await _mediator.Send(new GetContactSummaryQuery(contactId));

        if (contact.IsFailure)
            return Content(string.Empty);

        AddPhoneNumberToSchoolContactViewModel viewModel = new()
        {
            ContactId = contact.Value.ContactId,
            PhoneNumber = contact.Value.PhoneNumber
        };

        return Partial("AddPhoneNumberToSchoolContact", viewModel);
    }

    public async Task<IActionResult> OnPostContactPhoneUpdate(AddPhoneNumberToSchoolContactViewModel viewModel)
    {
        if (viewModel.PhoneNumber == PhoneNumber.Empty)
            return RedirectToPage();

        Result update = await _mediator.Send(new UpdateSchoolContactPhoneNumberCommand(viewModel.ContactId, viewModel.PhoneNumber));

        if (update.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                update.Error,
                _linkGenerator.GetPathByPage("/Emergency/Contacts/Index", values: new { area = "Admin" }));

            return Page();
        }

        return RedirectToPage();
    }
}