namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments;

using Application.Models.Auth;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetCurrentEnrolmentPeriods;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.PeriodSwitcher;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Partners_Enrolments_Offers_View_Value)]
public class OffersModel : BasePageModel
{
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

    public async Task OnGet()
    {
        Result<List<EnrolmentPeriodResponse>> periodsResult = await _mediator.Send(new GetCurrentEnrolmentPeriodsQuery());

        Periods = periodsResult.Value
            .Select(p => new PeriodSwitcherOption
            {
                PeriodId = p.Id,
                Label = p.Label,
                IsCurrent = false,
                Url = _linkGenerator.GetPathByPage("/Partner/Enrolments/Offers/Index", values: new { area = "Staff", PeriodId = p.Id })
            })
            .ToList();
    }

}
