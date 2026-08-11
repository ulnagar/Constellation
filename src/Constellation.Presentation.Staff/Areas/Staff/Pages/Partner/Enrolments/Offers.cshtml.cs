namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments;

using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetEnrolmentPeriodById;
using Application.Models.Auth;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetCurrentEnrolmentPeriods;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.PeriodSwitcher;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Partners_Enrolments_Offers_View_Value)]
public class OffersModel : BasePageModel
{
    private const string LastPeriodCookieName = "Enrolments.LastPeriodId";

    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public OffersModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Offers;
    [ViewData] public string PageTitle => "Enrolment Offers";

    public List<PeriodSwitcherOption> Periods { get; set; } = [];

    public async Task<IActionResult> OnGet()
    {
        if (Request.Cookies.TryGetValue(LastPeriodCookieName, out var raw)
            && Guid.TryParse(raw, out var guid))
        {
            EnrolmentPeriodId periodId = new EnrolmentPeriodId(guid);
            Result<EnrolmentPeriodResponse> result = await _mediator.Send(new GetEnrolmentPeriodByIdQuery(periodId));

            if (result.IsSuccess)
                return RedirectToPage("/Partner/Enrolments/Offers/Index", new { area = "Staff", PeriodId = periodId });
        }

        Result<List<EnrolmentPeriodResponse>> periodsResult = await _mediator.Send(new GetAllEnrolmentPeriodsQuery());

        if (periodsResult.IsFailure || periodsResult.Value.Count == 0)
            return RedirectToPage("/Partner/Enrolments/Periods/Index", new { area = "Staff" });

        if (periodsResult.Value.Count == 1)
            return RedirectToPage("/Partner/Enrolments/Offers/Index", new { area = "Staff", PeriodId = periodsResult.Value.First().Id });
        
        Periods = periodsResult.Value
            .Select(p => new PeriodSwitcherOption
            {
                PeriodId = p.Id,
                Label = p.Label,
                IsCurrent = p.Status <= PeriodStatus.Open,
                CurrentlySelected = false,
                Url = _linkGenerator.GetPathByPage("/Partner/Enrolments/Offers/Index", values: new { area = "Staff", PeriodId = p.Id })
            })
            .ToList();

        return Page();
    }

}
