namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Offers;

using Application.Common.PresentationModels;
using Application.Domains.EnrolmentContext.Offers.Commands.RecordParentResponseToOffer;
using Application.Domains.EnrolmentContext.Offers.Queries.GetOfferForResponse;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.EnrolmentContext.Offer.Errors;
using Core.Models.EnrolmentContext.Offer.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;
using OfferResponse = Application.Domains.EnrolmentContext.Offers.Queries.GetOfferForResponse.OfferResponse;

[HasPermission(AuthPermission.Partners_Enrolments_Offers_Edit_Value)]
public class RespondModel : PeriodScopedPageModel
{
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public RespondModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
        : base(mediator)
    {
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<RespondModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Offers;
    [ViewData] public string PageTitle => "Offer Response";

    [BindProperty(SupportsGet = true)]
    public OfferId Id { get; set; } = OfferId.Empty;

    public OfferResponse Offer { get; set; }

    [BindProperty]
    public string? OfferResponse { get; set; } = "Unset";
    [BindProperty] 
    public string? CourtOrders { get; set; } = "Unset";
    [BindProperty] 
    public string? HealthConditions { get; set; } = "Unset";
    [BindProperty]
    public string? LoanLaptop { get; set; } = "Unset";

    public async Task OnGet() => await PreparePage();
    
    private async Task PreparePage()
    {
        if (Id == OfferId.Empty)
        {
            _logger
                .ForContext(nameof(Error), EnrolmentOfferErrors.InvalidId, true)
                .Warning("Failed to retrieve Offer for response by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                EnrolmentOfferErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Offers/Index", values: new { area = "Staff", PeriodId }));

            return;
        }

        Result<OfferResponse> offer = await _mediator.Send(new GetOfferForResponseQuery(Id));

        if (offer.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), offer.Error, true)
                .Warning("Failed to retrieve Offer for response by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                offer.Error,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Offers/Index", values: new { area = "Staff", PeriodId }));

            return;
        }

        if (offer.Value.PeriodId != PeriodId)
        {
            _logger
                .ForContext(nameof(Error), EnrolmentPeriodErrors.PeriodMismatch, true)
                .Warning("Failed to retrieve Offer for response by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                EnrolmentPeriodErrors.PeriodMismatch,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Offers/Index", values: new { area = "Staff", PeriodId }));

            return;
        }

        Offer = offer.Value;

        if (OfferResponse != "Unset")
            return;

        OfferResponse = Offer.Response switch
        {
            var r when r == ResponseStatus.Accepted => "Accept",
            var r when r == ResponseStatus.Declined => "Decline",
            _ => "Unset"
        };
        CourtOrders = Offer.Response switch
        {
            var r when r == ResponseStatus.Accepted && Offer.HasCourtOrders => "Yes",
            var r when r == ResponseStatus.Accepted && !Offer.HasCourtOrders => "No",
            var r when r == ResponseStatus.Declined && Offer.HasCourtOrders => "Yes",
            var r when r == ResponseStatus.Declined && !Offer.HasCourtOrders => "No",
            _ => "Unset"
        };
        HealthConditions = Offer.Response switch
        {
            var r when r == ResponseStatus.Accepted && Offer.HasHealthConcerns => "Yes",
            var r when r == ResponseStatus.Accepted && !Offer.HasHealthConcerns => "No",
            var r when r == ResponseStatus.Declined && Offer.HasHealthConcerns => "Yes",
            var r when r == ResponseStatus.Declined && !Offer.HasHealthConcerns => "No",
            _ => "Unset"
        };
        LoanLaptop = Offer.Response switch
        {
            var r when r == ResponseStatus.Accepted && Offer.RequestedLaptop => "Yes",
            var r when r == ResponseStatus.Accepted && !Offer.RequestedLaptop => "No",
            var r when r == ResponseStatus.Declined && Offer.RequestedLaptop => "Yes",
            var r when r == ResponseStatus.Declined && !Offer.RequestedLaptop => "No",
            _ => "Unset"
        };
    }

    public async Task<IActionResult> OnPost()
    {
        if (OfferResponse == "Unset")
        {
            ModelState.AddModelError(nameof(OfferResponse),
                "You must either Accept or Decline the offer of enrolment to continue.");

            await PreparePage();
            return Page();
        }

        if (OfferResponse == "Accept" && CourtOrders == "Unset")
        {
            ModelState.AddModelError(nameof(CourtOrders),
                "You must select either Yes or No to the question about court orders.");

            await PreparePage();
            return Page();
        }

        if (OfferResponse == "Accept" && HealthConditions == "Unset")
        {
            ModelState.AddModelError(nameof(HealthConditions),
                "You must select either Yes or No to the question about health conditions.");

            await PreparePage();
            return Page();
        }

        if (OfferResponse == "Accept" && LoanLaptop == "Unset")
        {
            ModelState.AddModelError(nameof(LoanLaptop),
                "You must select either Yes or No to the question about a loan laptop.");

            await PreparePage();
            return Page();
        }

        RecordParentResponseToOfferCommand command = new(
            Id, 
            OfferResponse == "Accept" ? ResponseStatus.Accepted : ResponseStatus.Declined,
            CourtOrders == "Yes",
            HealthConditions == "Yes", 
            LoanLaptop == "Yes");

        _logger
            .ForContext(nameof(RecordParentResponseToOfferCommand), command, true)
            .Information("Requested to record parent response to offer by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(RecordParentResponseToOfferCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to record parent response to offer by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage("/Partner/Enrolments/Offers/Index", new { area = "Staff", PeriodId });
    }
}
