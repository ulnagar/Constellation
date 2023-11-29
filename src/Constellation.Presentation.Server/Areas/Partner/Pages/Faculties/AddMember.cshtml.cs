namespace Constellation.Presentation.Server.Areas.Partner.Pages.Faculties;

using Application.Faculties.GetFaculty;
using Application.StaffMembers.AddStaffToFaculty;
using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.Faculty.Identifiers;
using Core.Models.Faculty.ValueObjects;
using Core.Shared;
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
        public FacultyMembershipRole Role { get; set; }
    }

    [BindProperty(SupportsGet = true)]
    public Guid FacultyId { get; set; }

    [BindProperty]
    public AddMemberDto MemberDefinition { get; set; } = new();

    public Dictionary<string, string> StaffList { get; set; } = new();

    public SelectList FacultyRoles { get; set; } = new(FacultyMembershipRole.Enumerations(), "");

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        StaffList = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());
        MemberDefinition.FacultyId = FacultyId;
        
        FacultyId facultyId = Core.Models.Faculty.Identifiers.FacultyId.FromValue(FacultyId);

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
            FacultyId facultyId = Core.Models.Faculty.Identifiers.FacultyId.FromValue(MemberDefinition.FacultyId.Value);
            await _mediator.Send(new AddStaffToFacultyCommand(
                MemberDefinition.StaffId,
                facultyId,
                MemberDefinition.Role));
        }
        
        return RedirectToPage("Details", new { FacultyId = MemberDefinition.FacultyId });
    }
}
