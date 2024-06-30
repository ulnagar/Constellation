namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff.Faculties;

using Constellation.Application.Faculties.GetFacultyDetails;
using Constellation.Application.Models.Auth;
using Constellation.Application.StaffMembers.RemoveStaffFromFaculty;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Models.Faculties.Identifiers;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Faculties;

    [BindProperty(SupportsGet = true)]
    public Guid FacultyId { get; set; }

    public FacultyDetailsResponse Faculty { get; set; }

    public async Task OnGet()
    {
        FacultyId facultyId = Core.Models.Faculties.Identifiers.FacultyId.FromValue(FacultyId);

        Result<FacultyDetailsResponse> facultyRequest = await _mediator.Send(new GetFacultyDetailsQuery(facultyId));

        if (facultyRequest.IsSuccess)
            Faculty = facultyRequest.Value;
    }

    public async Task<IActionResult> OnPostRemoveMember([FromQuery]string staffId)
    {
        FacultyId facultyId = Core.Models.Faculties.Identifiers.FacultyId.FromValue(FacultyId);

        await _mediator.Send(new RemoveStaffFromFacultyCommand(staffId, facultyId));

        return RedirectToPage("/Partner/Staff/Faculties/Details", routeValues: new { FacultyId, area = "Staff" });
    }
}
