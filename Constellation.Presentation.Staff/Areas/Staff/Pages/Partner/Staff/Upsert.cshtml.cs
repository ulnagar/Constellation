namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff;

using Application.Models.Auth;
using Application.StaffMembers.CreateStaffMember;
using Application.StaffMembers.GetStaffById;
using Application.StaffMembers.UpdateStaffMember;
using Constellation.Application.Schools.GetSchoolsForSelectionList;
using Constellation.Application.Schools.Models;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanEditSchools)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Staff;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public string StaffId { get; set; } = string.Empty;

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string PortalUsername { get; set; } = string.Empty;

    [BindProperty]
    public string SchoolCode { get; set; } = string.Empty;

    [BindProperty]
    public bool IsShared { get; set; }

    public SelectList SchoolList { get; set; }

    public async Task OnGet()
    {
        Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

        if (schools.IsFailure)
        {
            Error = new()
            {
                Error = schools.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Staff/Index", values: new { area = "Staff" })
            };
            return;
        }

        if (string.IsNullOrWhiteSpace(Id))
        {
            SchoolList = new(schools.Value, "Code", "Name");

            return;
        }

        Result<StaffResponse> staffMember = await _mediator.Send(new GetStaffByIdQuery(Id));

        if (staffMember.IsFailure)
        {
            Error = new()
            {
                Error = staffMember.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Partner/Staff/Index", values: new { area = "Staff" })
            };

            return;
        }

        StaffId = staffMember.Value.StaffId;
        FirstName = staffMember.Value.Name.FirstName;
        LastName = staffMember.Value.Name.LastName;
        PortalUsername = staffMember.Value.PortalUsername;
        SchoolCode = staffMember.Value.SchoolCode;
        IsShared = staffMember.Value.IsShared;

        SchoolList = new(schools.Value, "Code", "Name", staffMember.Value.SchoolCode);
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

            SchoolList = new(schools.Value, "Code", "Name", SchoolCode);

            return Page();
        }

        if (string.IsNullOrWhiteSpace(Id))
        {
            // Create new student
            CreateStaffMemberCommand createCommand = new(
                StaffId,
                FirstName,
                LastName,
                PortalUsername,
                SchoolCode,
                IsShared);

            Result createResult = await _mediator.Send(createCommand);

            if (createResult.IsFailure)
            {
                Error = new()
                {
                    Error = createResult.Error,
                    RedirectPath = null
                };

                Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

                SchoolList = new(schools.Value, "Code", "Name", SchoolCode);

                return Page();
            }

            return RedirectToPage("/Partner/Staff/Index", new { area = "Staff" });
        }

        // Edit existing student
        UpdateStaffMemberCommand updateCommand = new(
            StaffId,
            FirstName,
            LastName,
            PortalUsername,
            SchoolCode,
            IsShared);

        Result updateResult = await _mediator.Send(updateCommand);

        if (updateResult.IsFailure)
        {
            Error = new()
            {
                Error = updateResult.Error,
                RedirectPath = null
            };

            Result<List<SchoolSelectionListResponse>> schools = await _mediator.Send(new GetSchoolsForSelectionListQuery());

            SchoolList = new(schools.Value, "Code", "Name", SchoolCode);

            return Page();
        }

        return RedirectToPage("/Partner/Staff/Index", new { area = "Staff" });
    }
}