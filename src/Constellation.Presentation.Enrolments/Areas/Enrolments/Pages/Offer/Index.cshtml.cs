namespace Constellation.Presentation.Enrolments.Areas.Enrolments.Pages.Offer;

using Application.Domains.EnrolmentContext.Offers.Queries.GetOfferForResponse;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
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
    private readonly LinkGenerator _linkGenerator;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public OfferId Id { get; set; }

    public OfferResponse Offer { get; set; }
    public bool Responded { get; set; }

    [BindProperty]
    public string? CourtOrders { get; set; } = "Unset";

    [BindProperty]
    public string? OfferResponse { get; set; } = "Unset";

    [BindProperty]
    public string? HealthConditions { get; set; } = "Unset";

    public async Task OnGet()
    {
        await PreparePage();
    }

    public async Task<IActionResult> OnPost()
    {
        if (OfferResponse == "Unset")
        {
            ModelState.AddModelError(nameof(OfferResponse), "You must either Accept or Decline the offer of enrolment to continue.");

            await PreparePage();
            return Page();
        }

        if (OfferResponse == "Decline")
        {
            // Process Decline response

            return RedirectToPage();
        }

        if (CourtOrders == "Unset")
        {
            ModelState.AddModelError(nameof(CourtOrders), "You must select either Yes or No to the question about court orders.");

            await PreparePage();
            return Page();
        }

        if (HealthConditions == "Unset")
        {
            ModelState.AddModelError(nameof(HealthConditions), "You must select either Yes or No to the question about health conditions.");

            await PreparePage();
            return Page();
        }

        // Process Accept response;

        return RedirectToPage();
    }

    private async Task PreparePage()
    {
        GetOfferForResponseQuery command = new(Id);

        _logger
            .ForContext(nameof(GetOfferForResponseQuery), command, true)
            .Information("Requested to retrieve Offer by user {User}", _currentUserService.UserName);

        Result<OfferResponse> offer = await _mediator.Send(command);

        if (offer.IsFailure)
        {
            _logger
                .ForContext(nameof(GetOfferForResponseQuery), command, true)
                .ForContext(nameof(Error), offer.Error, true)
                .Warning("Failed to retrieve Offer by user {User}", _currentUserService.UserName);

            return;
        }

        if (offer.Value.Status != OfferStatus.Pending)
            Responded = true;

        Offer = offer.Value;
    }
}