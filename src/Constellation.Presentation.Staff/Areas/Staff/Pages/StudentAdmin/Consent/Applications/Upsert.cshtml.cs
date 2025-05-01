namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Consent.Applications;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Domains.ThirdPartyConsent.Commands.CreateApplication;
using Constellation.Application.Domains.ThirdPartyConsent.Commands.UpdateApplication;
using Constellation.Application.Domains.ThirdPartyConsent.Queries.GetApplicationById;
using Core.Abstractions.Services;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Consent_Applications;
    [ViewData] public string PageTitle { get; set; } = "New Consent Application";

    
    [BindProperty(SupportsGet = true)]
    public ApplicationId Id { get; set; } = ApplicationId.Empty;

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
    public string ApplicationLink { get; set; } = string.Empty;

    [BindProperty]
    public bool ConsentRequired { get; set; }


    public async Task OnGet()
    {
        if (Id != ApplicationId.Empty)
        {
            _logger.Information("Requested to retrieve Consent Application with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            Result<ApplicationResponse> applicationRequest = await _mediator.Send(new GetApplicationByIdQuery(Id));

            if (applicationRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), applicationRequest.Error, true)
                    .Warning("Failed to retrieve Consent Application with id {Id} for edit by user {User}", Id, _currentUserService.UserName);
                
                ModalContent = new ErrorDisplay(
                    applicationRequest.Error,
                    _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Details", values: new { area = "Staff", Id = Id.Value }));

                return;
            }

            Name = applicationRequest.Value.Name;
            Purpose = applicationRequest.Value.Purpose;
            InformationCollected = applicationRequest.Value.InformationCollected.ToList();
            StoredCountry = applicationRequest.Value.StoredCountry;
            SharedWith = applicationRequest.Value.SharedWith.ToList();
            ConsentRequired = applicationRequest.Value.ConsentRequired;

            PageTitle = $"Editing - {Name}";
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

        if (Id != ApplicationId.Empty)
        {
            UpdateApplicationCommand command = new(
                Id,
                Name,
                Purpose,
                informationCollected,
                StoredCountry,
                sharedWith,
                ApplicationLink,
                ConsentRequired);

            _logger
                .ForContext(nameof(UpdateApplicationCommand), command, true)
                .Information("Requested to update Consent Application with id {Id} by user {User}", Id, _currentUserService.UserName);

            Result result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to update Consent Application with id {Id} by user {User}", Id, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(result.Error);

                return Page();
            }

            return RedirectToPage("/StudentAdmin/Consent/Applications/Details", new { area = "Staff", Id = Id.Value });
        }
        else
        {
            CreateApplicationCommand command = new(
                Name,
                Purpose,
                informationCollected,
                StoredCountry,
                sharedWith,
                ApplicationLink,
                ConsentRequired);

            _logger
                .ForContext(nameof(CreateApplicationCommand), command, true)
                .Information("Requested to create Consent Application by user {User}", _currentUserService.UserName);

            Result<ApplicationId> result = await _mediator.Send(command);

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to create Consent Application by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(result.Error);

                return Page();
            }

            return RedirectToPage("/StudentAdmin/Consent/Applications/Details", new { area = "Staff", Id = result.Value.Value });
        }
    }
}