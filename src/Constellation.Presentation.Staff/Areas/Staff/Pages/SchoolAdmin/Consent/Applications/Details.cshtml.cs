namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Consent.Applications;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.ThirdPartyConsent.DisableApplication;
using Application.ThirdPartyConsent.GetApplicationDetails;
using Application.ThirdPartyConsent.ReenableApplication;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Consent_Applications;
    [ViewData] public string PageTitle { get; set; } = "Consent Application Details";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public ApplicationId Id { get; set; }

    public ApplicationDetailsResponse Application { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve details for Consent Application with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<ApplicationDetailsResponse> applicationRequest = await _mediator.Send(new GetApplicationDetailsQuery(Id));

        if (applicationRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), applicationRequest.Error, true)
                .Warning("Failed to retrieve details for Consent Application with id {Id} by user {User}", Id, _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(
                applicationRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Applications/Index", values: new { area = "Staff" }));

            return;
        }

        Application = applicationRequest.Value;
        PageTitle = $"Details - {Application.Name}";
    }

    public async Task<IActionResult> OnGetDeactivate()
    {
        DisableApplicationCommand command = new(Id);

        _logger
            .ForContext(nameof(DisableApplicationCommand), command, true)
            .Information("Requested to disable Consent Application by user {User}", _currentUserService.UserName);

        Result applicationRequest = await _mediator.Send(command);

        if (applicationRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), applicationRequest.Error, true)
                .Warning("Failed to disable Consent Application by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                applicationRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Applications/Details", values: new { area = "Staff", Id = Id }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetReactivate()
    {
        ReenableApplicationCommand command = new(Id);

        _logger
            .ForContext(nameof(ReenableApplicationCommand), command, true)
            .Information("Requested to enable Consent Application by user {User}", _currentUserService.UserName);

        Result applicationRequest = await _mediator.Send(command);

        if (applicationRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), applicationRequest.Error, true)
                .Warning("Failed to enable Consent Application by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                applicationRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Applications/Details", values: new { area = "Staff", Id = Id }));

            return Page();
        }

        return RedirectToPage();
    }
}