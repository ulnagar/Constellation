namespace Constellation.Presentation.Server.Areas.Partner.Pages.Faculties;

using Constellation.Application.Features.Common.Queries;
using Constellation.Application.Features.Faculties.Queries;
using Constellation.Application.Features.StaffMembers.Commands;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        StaffList = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());
        MemberDefinition.FacultyId = FacultyId;
        MemberDefinition.FacultyName = await _mediator.Send(new GetFacultyNameQuery(FacultyId));
    }

    public async Task<IActionResult> OnPostAddMember()
    {
        if (!ModelState.IsValid)
        {
            StaffList = await _mediator.Send(new GetStaffMembersAsDictionaryQuery());

            return Page();
        }

        await _mediator.Send(new CreateFacultyMembershipForStaffMemberCommand
        {
            FacultyId = MemberDefinition.FacultyId.Value,
            StaffId = MemberDefinition.StaffId,
            Role = MemberDefinition.Role
        });

        return RedirectToPage("Details", new { FacultyId = MemberDefinition.FacultyId });
    }
}
