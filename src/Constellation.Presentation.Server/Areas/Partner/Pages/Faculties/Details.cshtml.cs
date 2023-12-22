namespace Constellation.Presentation.Server.Areas.Partner.Pages.Faculties;

using Application.Faculties.GetFacultyDetails;
using Application.StaffMembers.RemoveStaffFromFaculty;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.Faculty.Identifiers;
using Core.Shared;
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

    public FacultyDetailsResponse Faculty { get; set; }

    public async Task OnGet()
    {
        FacultyId facultyId = Core.Models.Faculty.Identifiers.FacultyId.FromValue(FacultyId);

        Result<FacultyDetailsResponse> facultyRequest = await _mediator.Send(new GetFacultyDetailsQuery(facultyId));

        if (facultyRequest.IsSuccess)
            Faculty = facultyRequest.Value;
    }

    public async Task<IActionResult> OnPostRemoveMember([FromQuery]string staffId)
    {
        FacultyId facultyId = Core.Models.Faculty.Identifiers.FacultyId.FromValue(FacultyId);

        await _mediator.Send(new RemoveStaffFromFacultyCommand(staffId, facultyId));

        return RedirectToPage("/Faculties/Details", routeValues: new { FacultyId, area = "Partner" });
    }
}
