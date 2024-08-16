namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Consent.Responses;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.ThirdPartyConsent.GetTransactionDetails;
using Core.Abstractions.Services;
using Core.Models.ThirdPartyConsent.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
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
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Consent_Transactions;
    [ViewData] public string PageTitle { get; set; } = "Consent Response Details";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(ConstructorBinder))]
    public ConsentTransactionId Id { get; set; } = ConsentTransactionId.Empty;

    public TransactionDetailsResponse Transaction { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve details of Consent Transaction by user {User}", _currentUserService.UserName);

        Result<TransactionDetailsResponse> result = await _mediator.Send(new GetTransactionDetailsQuery(Id));

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to retrieve details of Consent Transaction by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                result.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Consent/Responses/Index", values: new { area = "Staff" }));

            return;
        }

        Transaction = result.Value;
    }
}