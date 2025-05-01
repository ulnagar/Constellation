namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Consent.Applications;

using Application.Common.PresentationModels;
using Application.Domains.ThirdPartyConsent.Commands.RevokeRequirement;
using Application.Models.Auth;
using Constellation.Application.Domains.ThirdPartyConsent.Commands.DisableApplication;
using Constellation.Application.Domains.ThirdPartyConsent.Commands.ReenableApplication;
using Constellation.Application.Domains.ThirdPartyConsent.Queries.GetApplicationDetails;
using Core.Abstractions.Services;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Consent_Applications;
    [ViewData] public string PageTitle { get; set; } = "Consent Application Details";

    [BindProperty(SupportsGet = true)]
    public ApplicationId Id { get; set; }

    public ApplicationDetailsResponse Application { get; set; }

    public async Task OnGet() => await PreparePage();

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
                _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

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
                _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetRevokeRequirement(ConsentRequirementId requirementId)
    {
        Result result = await _mediator.Send(new RevokeRequirementCommand(Id, requirementId));

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to revoke Consent Requirement by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(result.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    private async Task PreparePage()
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
                _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Index", values: new { area = "Staff" }));

            return;
        }

        Application = applicationRequest.Value;
        PageTitle = $"Details - {Application.Name}";
    }
}