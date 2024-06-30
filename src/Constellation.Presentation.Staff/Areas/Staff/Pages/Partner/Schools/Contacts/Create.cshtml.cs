namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools.Contacts;

using Constellation.Application.Helpers;
using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContact;
using Constellation.Application.SchoolContacts.CreateContactWithRole;
using Constellation.Application.SchoolContacts.GetContactRolesForSelectionList;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Models.SchoolContacts.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanManageSchoolContacts)]
public class CreateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public CreateModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Contacts;

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

    [BindProperty]
    public string? SchoolCode { get; set; }
    [BindProperty]
    public string? Role { get; set; }
    [BindProperty]
    public string? Note { get; set; }

    public SelectList SchoolsList { get; set; }
    public SelectList RolesList { get; set; }

    public async Task OnGet()
    {
        Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
        Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

        SchoolsList = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
        RolesList = new SelectList(rolesRequest.Value);
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
            Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

            SchoolsList = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
            RolesList = new SelectList(rolesRequest.Value);

            return Page();
        }

        FirstName = FirstName.Trim();
        LastName = LastName.Trim();
        PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? PhoneNumber : PhoneNumber.Trim();
        EmailAddress = EmailAddress.Trim();
        
        Role = string.IsNullOrWhiteSpace(Role) ? string.Empty : Role.Trim();
        Note = string.IsNullOrWhiteSpace(Note) ? Note : Note.Trim();
        
        if (string.IsNullOrWhiteSpace(SchoolCode))
        {
            Result<SchoolContactId> request = await _mediator.Send(new CreateContactCommand(
                FirstName,
                LastName,
                EmailAddress,
                PhoneNumber,
                false));

            if (request.IsFailure)
            {
                Error = new()
                {
                    Error = request.Error,
                    RedirectPath = null
                };

                Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
                Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

                SchoolsList = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
                RolesList = new SelectList(rolesRequest.Value);

                return Page();
            }
        }
        else
        {
            Result request = await _mediator.Send(new CreateContactWithRoleCommand(
                FirstName,
                LastName,
                EmailAddress,
                PhoneNumber,
                Role,
                SchoolCode,
                Note,
                false));

            if (request.IsFailure)
            {
                Error = new()
                {
                    Error = request.Error,
                    RedirectPath = null
                };

                Result<List<string>> rolesRequest = await _mediator.Send(new GetContactRolesForSelectionListQuery());
                Result<List<SchoolSelectionListResponse>> schoolsRequest = await _mediator.Send(new GetSchoolsForSelectionListQuery(GetSchoolsForSelectionListQuery.SchoolsFilter.PartnerSchools));

                SchoolsList = new SelectList(schoolsRequest.Value.OrderBy(entry => entry.Name), "Code", "Name");
                RolesList = new SelectList(rolesRequest.Value);

                return Page();
            }
        }
       
        return RedirectToPage("/Partner/Schools/Contacts/Index", new { area = "Staff" });
    }
}