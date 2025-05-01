namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Consent.Responses;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Domains.ThirdPartyConsent.Queries.GetConsentDetails;
using Core.Abstractions.Services;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Consent_Transactions;
    [ViewData] public string PageTitle { get; set; } = "Consent Response Details";

    [BindProperty(SupportsGet = true)]
    public ConsentId ConsentId { get; set; } = ConsentId.Empty;
    [BindProperty(SupportsGet = true)]
    public ApplicationId ApplicationId { get; set; } = ApplicationId.Empty;

    public ConsentDetailsResponse Transaction { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve details of Consent by user {User}", _currentUserService.UserName);

        Result<ConsentDetailsResponse> result = await _mediator.Send(new GetConsentDetailsQuery(ApplicationId, ConsentId));

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to retrieve details of Consent by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Responses/Index", values: new { area = "Staff" }));

            return;
        }

        Transaction = result.Value;
    }
}