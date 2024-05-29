namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Consent.Applications;

using Application.Models.Auth;
using Application.ThirdPartyConsent.CreateApplication;
using Application.ThirdPartyConsent.GetApplicationById;
using Application.ThirdPartyConsent.UpdateApplication;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Consent_Applications;

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [BindProperty]
    [Required]
    public string Purpose { get; set; } = string.Empty;

    [BindProperty] 
    public List<string> InformationCollected { get; set; } = new() { string.Empty };
    
    [BindProperty]
    public string StoredCountry { get; set; } = string.Empty;

    [BindProperty] 
    public List<string> SharedWith { get; set; } = new() { string.Empty };
    
    [BindProperty]
    public bool ConsentRequired { get; set; }


    public async Task OnGet()
    {
        if (Id.HasValue)
        {
            ApplicationId applicationId = ApplicationId.FromValue(Id.Value);

            Result<ApplicationResponse> applicationRequest = await _mediator.Send(new GetApplicationByIdQuery(applicationId));

            if (applicationRequest.IsFailure)
            {
                Error = new()
                {
                    Error = applicationRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Applications/Details", values: new { area = "Staff", Id = Id.Value })
                };

                return;
            }

            Name = applicationRequest.Value.Name;
            Purpose = applicationRequest.Value.Purpose;
            InformationCollected = applicationRequest.Value.InformationCollected.ToList();
            StoredCountry = applicationRequest.Value.StoredCountry;
            SharedWith = applicationRequest.Value.SharedWith.ToList();
            ConsentRequired = applicationRequest.Value.ConsentRequired;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        string[] informationCollected = InformationCollected
            .Where(entry => !string.IsNullOrWhiteSpace(entry))
            .ToArray();

        if (informationCollected.Length == 0)
        {
            ModelState.AddModelError("InformationCollected", "You must include at least one non-blank entry");
        }

        string[] sharedWith = SharedWith
            .Where(entry => !string.IsNullOrWhiteSpace(entry))
            .ToArray();

        if (sharedWith.Length == 0)
        {
            ModelState.AddModelError("SharedWith", "You must include at least one non-blank entry");
        }

        if (!ModelState.IsValid)
            return Page();

        if (Id.HasValue)
        {
            ApplicationId applicationId = ApplicationId.FromValue(Id.Value);

            UpdateApplicationCommand command = new(
                applicationId,
                Name,
                Purpose,
                informationCollected,
                StoredCountry,
                sharedWith,
                ConsentRequired);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = null
                };

                return Page();
            }

            return RedirectToPage("/SchoolAdmin/Consent/Applications/Details", new { area = "Staff", Id = Id.Value });
        }
        else
        {
            CreateApplicationCommand command = new(
                Name,
                Purpose,
                informationCollected,
                StoredCountry,
                sharedWith,
                ConsentRequired);

            Result<ApplicationId> result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = null
                };

                return Page();
            }

            return RedirectToPage("/SchoolAdmin/Consent/Applications/Details", new { area = "Staff", Id = result.Value.Value });
        }
    }
}