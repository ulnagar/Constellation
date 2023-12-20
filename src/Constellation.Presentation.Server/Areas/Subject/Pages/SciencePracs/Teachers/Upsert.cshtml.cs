namespace Constellation.Presentation.Server.Areas.Subject.Pages.SciencePracs.Teachers;

using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContactWithRole;
using Constellation.Application.SchoolContacts.GetContactSummary;
using Constellation.Application.SchoolContacts.UpdateContact;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

    [ViewData] public string ActivePage => SubjectPages.Teachers;

    [BindProperty(SupportsGet = true)]
    public int? Id { get; set; }

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
        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            Result<ContactSummaryResponse> contactRequest = await _mediator.Send(new GetContactSummaryQuery(Id.Value));
            
            if (contactRequest.IsFailure)
            {
                Error = new()
                {
                    Error = contactRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SciencePracs/Teachers/Index", values: new { area = "Subject" })
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
            Result updateRequest = await _mediator.Send(new UpdateContactCommand(
                Id.Value,
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

            return RedirectToPage("/SciencePracs/Teachers/Index", new { area = "Subject" });
        }

        CreateContactWithRoleCommand command = new(
            FirstName,
            LastName,
            EmailAddress,
            PhoneNumber,
            Role,
            SchoolCode,
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

        return RedirectToPage("/SciencePracs/Teachers/Index", new { area = "Subject" });
    }
}
