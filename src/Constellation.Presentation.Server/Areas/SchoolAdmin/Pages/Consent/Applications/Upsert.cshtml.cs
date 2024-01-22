namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Consent.Applications;

using Application.Models.Auth;
using Application.ThirdPartyConsent.GetApplicationById;
using BaseModels;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [ViewData] public string ActivePage => ConsentPages.Applications;

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [BindProperty]
    [Required]
    public string Purpose { get; set; } = string.Empty;
    
    [BindProperty]
    public string[] InformationCollected { get; set; }
    
    [BindProperty]
    public string StoredCountry { get; set; } = string.Empty;
    
    [BindProperty] 
    public string[] SharedWith { get; set; }
    
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
                    RedirectPath = _linkGenerator.GetPathByPage("/Consent/Applications/Details", values: new { area = "SchoolAdmin", Id = Id.Value })
                };

                return;
            }

            Name = applicationRequest.Value.Name;
            Purpose = applicationRequest.Value.Purpose;
            InformationCollected = applicationRequest.Value.InformationCollected;
            StoredCountry = applicationRequest.Value.StoredCountry;
            SharedWith = applicationRequest.Value.SharedWith;
            ConsentRequired = applicationRequest.Value.ConsentRequired;
        }
    }

    public async Task<IActionResult> OnPost()
    {

    }
}