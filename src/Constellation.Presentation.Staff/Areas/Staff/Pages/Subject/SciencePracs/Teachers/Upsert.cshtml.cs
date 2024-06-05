namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Teachers;

using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContactWithRole;
using Constellation.Application.SchoolContacts.GetContactSummary;
using Constellation.Application.SchoolContacts.UpdateContact;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanManageSciencePracs)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Teachers;

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    [Required]
    public string FirstName { get; set; }

    [BindProperty]
    [Required]
    public string LastName { get; set; }

    [BindProperty]
    [Required]
    [DataType(DataType.EmailAddress)]
    public string EmailAddress { get; set; }

    [BindProperty]
    [DataType(DataType.PhoneNumber)]
    public string PhoneNumber { get; set; }

    [BindProperty]
    public string SchoolCode { get; set; }

    [BindProperty]
    public string Role { get; set; }

    public SelectList SchoolList { get; set; }
    
    public async Task OnGet()
    {
        if (Id.HasValue)
        {
            SchoolContactId contactId = SchoolContactId.FromValue(Id.Value);
            
            Result<ContactSummaryResponse> contactRequest = await _mediator.Send(new GetContactSummaryQuery(contactId));
            
            if (contactRequest.IsFailure)
            {
                Error = new()
                {
                    Error = contactRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Subject/SciencePracs/Teachers/Index", values: new { area = "Staff" })
                };

                return;
            }

            FirstName = contactRequest.Value.FirstName;
            LastName = contactRequest.Value.LastName;
            EmailAddress = contactRequest.Value.EmailAddress;
            PhoneNumber = contactRequest.Value.PhoneNumber;
        }

        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        SchoolList = new SelectList(schoolsRequest.Value, "Code", "Name");
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!Id.HasValue && string.IsNullOrWhiteSpace(SchoolCode))
            ModelState.AddModelError("SchoolCode", "You must select a school");

        if (!ModelState.IsValid)
        {
            Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery());

            SchoolList = new SelectList(schoolsRequest.Value, "Code", "Name");

            return Page();
        }

        if (Id.HasValue)
        {
            SchoolContactId contactId = SchoolContactId.FromValue(Id.Value);
            
            Result updateRequest = await _mediator.Send(new UpdateContactCommand(
                contactId,
                FirstName,
                LastName,
                EmailAddress,
                PhoneNumber));

            if (updateRequest.IsFailure)
            {
                Error = new()
                {
                    Error = updateRequest.Error,
                    RedirectPath = null
                };

                return Page();
            }

            return RedirectToPage("/Subject/SciencePracs/Teachers/Index", new { area = "Staff" });
        }

        CreateContactWithRoleCommand command = new(
            FirstName,
            LastName,
            EmailAddress,
            PhoneNumber,
            Role,
            SchoolCode,
            string.Empty,
            false);

        Result createRequest = await _mediator.Send(command);

        if (createRequest.IsFailure)
        {
            Error = new()
            {
                Error = createRequest.Error,
                RedirectPath = null
            };

            Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery());

            SchoolList = new SelectList(schoolsRequest.Value, "Code", "Name");
                
            return Page();
        }

        return RedirectToPage("/Subject/SciencePracs/Teachers/Index", new { area = "Staff" });
    }
}
