namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Staff.Faculties;

using Constellation.Application.Faculties.GetFacultiesSummary;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly IMediator _mediator;

    public IndexModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Staff_Faculties;

    public List<FacultySummaryResponse> Faculties { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<FacultySummaryResponse>> faculties = await _mediator.Send(new GetFacultiesSummaryQuery());

        if (faculties.IsFailure)
        {
            Error = new()
            {
                Error = faculties.Error,
                RedirectPath = null
            };

            return;
        }

        Faculties = faculties.Value;
    }
}
