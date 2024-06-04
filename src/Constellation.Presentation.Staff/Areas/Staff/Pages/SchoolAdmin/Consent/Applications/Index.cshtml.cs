namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Consent.Applications;

using Application.Models.Auth;
using Application.ThirdPartyConsent.GetApplications;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Consent_Applications;

    public List<ApplicationSummaryResponse> Applications { get; set; }

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Active;

    public async Task OnGet()
    {
        Result<List<ApplicationSummaryResponse>> applicationsRequest = await _mediator.Send(new GetApplicationsQuery());

        if (applicationsRequest.IsFailure)
        {
            Error = new()
            {
                Error = applicationsRequest.Error,
                RedirectPath = null
            };

            return;
        }

        Applications = Filter switch
        {
            FilterDto.Active => applicationsRequest.Value.Where(entry => !entry.IsDeleted).ToList(),
            FilterDto.Inactive => applicationsRequest.Value.Where(entry => entry.IsDeleted).ToList(),
            FilterDto.All => applicationsRequest.Value
        };

        Applications = Applications.OrderBy(response => response.Name).ToList();
    }

    public enum FilterDto
    {
        All,
        Active,
        Inactive
    }
}