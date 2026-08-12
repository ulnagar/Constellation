namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Offers;

using Application.Common.PresentationModels;
using Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferAcceptedByStaff;
using Application.Domains.EnrolmentContext.Offers.Commands.MarkOfferDeclinedByStaff;
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

        OfferResponse = Offer.Status switch
        {
            OfferStatus.Accepted => "Accept",
            OfferStatus.Declined => "Decline",
            _ => "Unset"
        };
        CourtOrders = Offer.Status switch
        {
            OfferStatus.Accepted when Offer.HasCourtOrders => "Yes",
            OfferStatus.Accepted when !Offer.HasCourtOrders => "No",
            OfferStatus.Declined when Offer.HasCourtOrders => "Yes",
            OfferStatus.Declined when !Offer.HasCourtOrders => "No",
            _ => "Unset"
        };
        HealthConditions = Offer.Status switch
        {
            OfferStatus.Accepted when Offer.HasHealthConcerns => "Yes",
            OfferStatus.Accepted when !Offer.HasHealthConcerns => "No",
            OfferStatus.Declined when Offer.HasHealthConcerns => "Yes",
            OfferStatus.Declined when !Offer.HasHealthConcerns => "No",
            _ => "Unset"
        };
        LoanLaptop = Offer.Status switch
        {
            OfferStatus.Accepted when Offer.RequestedLaptop => "Yes",
            OfferStatus.Accepted when !Offer.RequestedLaptop => "No",
            OfferStatus.Declined when Offer.RequestedLaptop=> "Yes",
            OfferStatus.Declined when !Offer.RequestedLaptop => "No",
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

        if (OfferResponse == "Decline")
        {
            MarkOfferDeclinedByStaffCommand declinedCommand = new(Id);

            _logger
                .ForContext(nameof(MarkOfferDeclinedByStaffCommand), declinedCommand, true)
                .Information("Requested to mark Offer declined by user {User}", _currentUserService.UserName);

            Result declinedResult = await _mediator.Send(declinedCommand);

            if (declinedResult.IsFailure)
            {
                _logger
                    .ForContext(nameof(MarkOfferDeclinedByStaffCommand), declinedCommand, true)
                    .ForContext(nameof(Error), declinedResult.Error, true)
                    .Warning("Failed to mark Offer declined by user {User}", _currentUserService.UserName);

                ModalContent = ErrorDisplay.Create(declinedResult.Error);

                await PreparePage();
                return Page();
            }

            return RedirectToPage("/Partner/Enrolments/Offers/Index", new { area = "Staff", PeriodId });
        }

        if (CourtOrders == "Unset")
        {
            ModelState.AddModelError(nameof(CourtOrders),
                "You must select either Yes or No to the question about court orders.");

            await PreparePage();
            return Page();
        }

        if (HealthConditions == "Unset")
        {
            ModelState.AddModelError(nameof(HealthConditions),
                "You must select either Yes or No to the question about health conditions.");

            await PreparePage();
            return Page();
        }

        MarkOfferAcceptedByStaffCommand acceptedCommand = new(Id, CourtOrders == "Yes", HealthConditions == "Yes", LoanLaptop == "Yes");

        _logger
            .ForContext(nameof(MarkOfferAcceptedByStaffCommand), acceptedCommand, true)
            .Information("Requested to mark Offer accepted by user {User}", _currentUserService.UserName);

        Result acceptedResult = await _mediator.Send(acceptedCommand);

        if (acceptedResult.IsFailure)
        {
            _logger
                .ForContext(nameof(MarkOfferAcceptedByStaffCommand), acceptedCommand, true)
                .ForContext(nameof(Error), acceptedResult.Error, true)
                .Warning("Failed to mark Offer accepted by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(acceptedResult.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage("/Partner/Enrolments/Offers/Index", new { area = "Staff", PeriodId });
    }
}
