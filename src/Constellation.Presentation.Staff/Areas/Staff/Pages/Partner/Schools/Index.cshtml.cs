namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools;

using Application.Models.Auth;
using Application.Schools.GetSchoolsSummaryList;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Schools;

    [BindProperty(SupportsGet = true)]
    public SchoolFilter Filter { get; set; } = SchoolFilter.Active;

    public List<SchoolSummaryResponse> Schools { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<SchoolSummaryResponse>> result = await _mediator.Send(new GetSchoolsSummaryListQuery(Filter));

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = null
            };

            return;
        }

        Schools = result.Value;
    }
}