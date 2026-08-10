namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Offer;

using Application.Domains.EnrolmentContext.Offers.Commands.AcceptOffer;
using Application.Domains.EnrolmentContext.Offers.Commands.DeclineOffer;
using Application.Domains.EnrolmentContext.Offers.Queries.GetOfferForResponse;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.EnrolmentContext.Offer.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Serilog;

[AllowAnonymous]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeProvider _dateTime;
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        IDateTimeProvider dateTime,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _linkGenerator = linkGenerator;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? Token { get; set; }
    
    [BindProperty]
    public OfferId Id { get; set; }

    public OfferResponse Offer { get; set; }
    public bool Responded { get; set; }

    [BindProperty]
    public string? CourtOrders { get; set; } = "Unset";

    [BindProperty]
    public string? OfferResponse { get; set; } = "Unset";

    [BindProperty]
    public string? HealthConditions { get; set; } = "Unset";

    [BindProperty]
    public string? LoanLaptop { get; set; } = "Unset";

    public async Task<IActionResult> OnGet(CancellationToken cancellationToken = default)
    {
        IActionResult redirect = await PreparePage(cancellationToken);
        
        return redirect;
    }

    public async Task<IActionResult> OnPost(CancellationToken cancellationToken = default)
    {
        if (OfferResponse == "Unset")
        {
            ModelState.AddModelError(nameof(OfferResponse), "You must either Accept or Decline the offer of enrolment to continue.");

            IActionResult redirect = await PreparePage(cancellationToken);
            return redirect;
        }

        if (OfferResponse == "Decline")
        {
            DeclineOfferCommand declineCommand = new(Id);

            _logger
                .ForContext(nameof(DeclineOfferCommand), declineCommand, true)
                .Information("Requested to decline Offer by user {User}", _currentUserService.UserName);

            Result declineResult = await _mediator.Send(declineCommand, cancellationToken);

            if (declineResult.IsFailure)
            {
                _logger
                    .ForContext(nameof(DeclineOfferCommand), declineCommand, true)
                    .ForContext(nameof(Error), declineResult.Error, true)
                    .Warning("Failed to decline Offer by user {User}", _currentUserService.UserName);

                IActionResult redirect = await PreparePage(cancellationToken);
                return redirect;
            }

            return RedirectToPage();
        }

        if (CourtOrders == "Unset")
        {
            ModelState.AddModelError(nameof(CourtOrders), "You must select either Yes or No to the question about court orders.");

            IActionResult redirect = await PreparePage(cancellationToken);
            return redirect;
        }

        if (HealthConditions == "Unset")
        {
            ModelState.AddModelError(nameof(HealthConditions), "You must select either Yes or No to the question about health conditions.");

            IActionResult redirect = await PreparePage(cancellationToken);
            return redirect;
        }

        if (LoanLaptop == "Unset")
        {
            ModelState.AddModelError(nameof(LoanLaptop), "You must select either Yes or No to the question about borrowing a laptop.");

            IActionResult redirect = await PreparePage(cancellationToken);
            return redirect;
        }

        AcceptOfferCommand acceptCommand = new(
            Id,
            CourtOrders == "Yes",
            HealthConditions == "Yes",
            LoanLaptop == "Yes");

        _logger
            .ForContext(nameof(AcceptOfferCommand), acceptCommand, true)
            .Information("Requested to accept Offer by user {User}", _currentUserService.UserName);

        Result acceptResult = await _mediator.Send(acceptCommand, cancellationToken);

        if (acceptResult.IsFailure)
        {
            _logger
                .ForContext(nameof(AcceptOfferCommand), acceptCommand, true)
                .ForContext(nameof(Error), acceptResult.Error, true)
                .Warning("Failed to accept Offer by user {User}", _currentUserService.UserName);

            IActionResult redirect = await PreparePage(cancellationToken);
            return redirect;
        }

        return RedirectToPage();
    }

    private async Task<IActionResult> PreparePage(CancellationToken cancellationToken = default)
    {
        if (!Guid.TryParse(Token, out var offerGuid))
            return RedirectToExpired();

        Id = OfferId.FromValue(offerGuid);

        GetOfferForResponseQuery command = new(Id);

        _logger
            .ForContext(nameof(GetOfferForResponseQuery), command, true)
            .Information("Requested to retrieve Offer by user {User}", _currentUserService.UserName);

        Result<OfferResponse> offer = await _mediator.Send(command, cancellationToken);

        if (offer.IsFailure || (offer.Value.Status == OfferStatus.Pending && offer.Value.RespondBy < _dateTime.Now))
        {
            _logger
                .ForContext(nameof(GetOfferForResponseQuery), command, true)
                .ForContext(nameof(Error), offer.Error, true)
                .Warning("Failed to retrieve Offer by user {User}", _currentUserService.UserName);

            return RedirectToExpired();
        }

        if (offer.Value.Status != OfferStatus.Pending)
            Responded = true;

        Offer = offer.Value;
        if (Offer.RespondedAt.HasValue)
        {
            OfferResponse = Offer.Status switch
            {
                OfferStatus.Accepted => "Accept",
                OfferStatus.Declined => "Decline",
                _ => "Unset"
            };

            CourtOrders = Offer.HasCourtOrders switch
            {
                true => "Yes",
                false => "No"
            };

            HealthConditions = Offer.HasHealthConcerns switch
            {
                true => "Yes",
                false => "No"
            };

            LoanLaptop = Offer.RequestedLaptop switch
            {
                true => "Yes",
                false => "No"
            };
        }

        return Page();
    }

    private IActionResult RedirectToExpired() =>
        RedirectToPage("/Error", new { area = "Enrolments" });
}