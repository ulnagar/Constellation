namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff.Faculties;

using Constellation.Application.Faculties.GetFaculty;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Application.StaffMembers.AddStaffToFaculty;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Models.Faculties.Identifiers;
using Core.Models.Faculties.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditFaculties)]
public class AddMemberModel : BasePageModel
{
    private readonly IMediator _mediator;

    public AddMemberModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public class AddMemberDto
    {
        [Required]
        public Guid? FacultyId { get; set; }
        public string FacultyName { get; set; }
        [Required]
        public string StaffId { get; set; }
        [Required]
        public string Role { get; set; }
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Faculties;

    [BindProperty(SupportsGet = true)]
    public Guid FacultyId { get; set; }

    [BindProperty]
    public AddMemberDto MemberDefinition { get; set; } = new();

    public Dictionary<string, string> StaffList { get; set; } = new();

    public SelectList FacultyRoles { get; set; } = new(FacultyMembershipRole.Enumerations(), "");

    public async Task OnGet()
    {
        StaffList = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());
        MemberDefinition.FacultyId = FacultyId;
        
        FacultyId facultyId = Core.Models.Faculties.Identifiers.FacultyId.FromValue(FacultyId);

        Result<FacultyResponse> faculty = await _mediator.Send(new GetFacultyQuery(facultyId));

        if (faculty.IsSuccess)
            MemberDefinition.FacultyName = faculty.Value.Name;
    }

    public async Task<IActionResult> OnPostAddMember()
    {
        if (!ModelState.IsValid)
        {
            StaffList = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

            return Page();
        }

        if (MemberDefinition.FacultyId.HasValue)
        {
            FacultyMembershipRole role = FacultyMembershipRole.FromValue(MemberDefinition.Role);

            FacultyId facultyId = Core.Models.Faculties.Identifiers.FacultyId.FromValue(MemberDefinition.FacultyId.Value);
            await _mediator.Send(new AddStaffToFacultyCommand(
                MemberDefinition.StaffId,
                facultyId,
                role));
        }
        
        return RedirectToPage("/Partner/Staff/Faculties/Details", new { FacultyId = MemberDefinition.FacultyId, area="Staff"});
    }
}
