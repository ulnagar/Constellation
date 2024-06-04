namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Consent.Applications;

using Application.Models.Auth;
using Application.ThirdPartyConsent.DisableApplication;
using Application.ThirdPartyConsent.GetApplicationDetails;
using Application.ThirdPartyConsent.ReenableApplication;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Consent_Applications;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ApplicationDetailsResponse Application { get; set; }

    public async Task OnGet()
    {
        ApplicationId applicationId = ApplicationId.FromValue(Id);

        Result<ApplicationDetailsResponse> applicationRequest = await _mediator.Send(new GetApplicationDetailsQuery(applicationId));

        if (applicationRequest.IsFailure)
        {
            Error = new()
            {
                Error = applicationRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Applications/Index", values: new { area = "Staff" })
            };

            return;
        }

        Application = applicationRequest.Value;
    }

    public async Task<IActionResult> OnGetDeactivate()
    {
        ApplicationId applicationId = ApplicationId.FromValue(Id);

        Result applicationRequest = await _mediator.Send(new DisableApplicationCommand(applicationId));

        if (applicationRequest.IsFailure)
        {
            Error = new()
            {
                Error = applicationRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Applications/Details", values: new { area = "Staff", Id = Id })
            };

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetReactivate()
    {
        ApplicationId applicationId = ApplicationId.FromValue(Id);

        Result applicationRequest = await _mediator.Send(new ReenableApplicationCommand(applicationId));

        if (applicationRequest.IsFailure)
        {
            Error = new()
            {
                Error = applicationRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Applications/Details", values: new { area = "Staff", Id = Id })
            };

            return Page();
        }

        return RedirectToPage();
    }
}