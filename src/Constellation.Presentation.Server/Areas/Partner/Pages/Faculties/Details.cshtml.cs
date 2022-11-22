namespace Constellation.Presentation.Server.Areas.Partner.Pages.Faculties;

using Constellation.Application.Features.Faculties.Models;
using Constellation.Application.Features.Faculties.Queries;
using Constellation.Application.Features.StaffMembers.Commands;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanViewFacultyDetails)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;

    public DetailsModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid FacultyId { get; set; }

    public FacultyDetailsDto Faculty { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        Faculty = await _mediator.Send(new GetFacultyDetailsQuery(FacultyId));
    }

    public async Task<IActionResult> OnPostRemoveMember([FromQuery]Guid membershipId)
    {
        await _mediator.Send(new RemoveFacultyMembershipFromStaffMemberCommand { MembershipId = membershipId });

        await GetClasses(_mediator);
        Faculty = await _mediator.Send(new GetFacultyDetailsQuery(FacultyId));
        return Page();
    }
}
