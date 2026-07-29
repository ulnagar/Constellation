namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Offers;

using Application.Domains.EnrolmentContext.Offers.Models;
using Application.Domains.EnrolmentContext.Offers.Queries.ExportOfferList;
using Application.Domains.EnrolmentContext.Offers.Queries.GetOffersForPeriod;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;
using Constellation.Application.Helpers;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Constellation.Core.Shared;
using Core.Models.EnrolmentContext.Offer.Enums;
using Core.Models.EnrolmentContext.Offer.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.Partners_Enrolments_Offers_View_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Offers;
    [ViewData] public string PageTitle => "Enrolment Offers";

    [BindProperty(SupportsGet = true)]
    public EnrolmentPeriodId PeriodId { get; set; } = EnrolmentPeriodId.Empty;
    [BindProperty(SupportsGet = true)]
    public StatusFilter Status { get; set; } = StatusFilter.All;
    public List<EnrolmentPeriodResponse> Periods { get; set; } = [];
    public List<EnrolmentOfferResponse> Offers { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        return await PreparePage();
    }

    public async Task<IActionResult> PreparePage()
    {
        Result<List<EnrolmentPeriodResponse>> periods = await _mediator.Send(new GetAllEnrolmentPeriodsQuery());

        if (periods.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(periods.Error);

            return Page();
        }

        Periods = periods.Value
            .OrderBy(entry => entry.OpenAt)
            .ToList();

        if (PeriodId == EnrolmentPeriodId.Empty)
        {
            if (Periods.Count is 0 or > 1)
                return Page();

            return RedirectToPage(new { PeriodId = Periods.First().Id });
        }

        Result<List<EnrolmentOfferResponse>> offers = await _mediator.Send(new GetOffersForPeriodQuery(PeriodId));

        if (offers.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(offers.Error);

            return Page();
        }

        Offers = FilterOffers(offers.Value, Status);

        return Page();
    }

    public async Task<IActionResult> OnPostExport(List<OfferId> offerIds)
    {
        Result<byte[]> file = await _mediator.Send(new ExportOfferListQuery(offerIds));

        if (file.IsFailure)
            return BadRequest(file.Error.Message);

        return File(file.Value, FileContentTypes.ExcelModernFile, "Enrolment Offer Export.xlsx");
    }

    public enum StatusFilter
    {
        All,
        Processing,
        Pending,
        Accepted,
        Declined
    }

    private static List<EnrolmentOfferResponse> FilterOffers(
        IEnumerable<EnrolmentOfferResponse> offers,
        StatusFilter filter)
    {
        return filter switch
        {
            StatusFilter.All => offers.ToList(),

            StatusFilter.Processing => offers
                .Where(offer => offer.Status == OfferStatus.Processing)
                .ToList(),

            StatusFilter.Pending => offers
                .Where(offer => offer.Status == OfferStatus.Pending || offer.Status == OfferStatus.Lapsed)
                .ToList(),

            StatusFilter.Accepted => offers
                .Where(offer => offer.Status == OfferStatus.Accepted)
                .ToList(),

            StatusFilter.Declined => offers
                .Where(offer => offer.Status == OfferStatus.Declined)
                .ToList(),

            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }
}