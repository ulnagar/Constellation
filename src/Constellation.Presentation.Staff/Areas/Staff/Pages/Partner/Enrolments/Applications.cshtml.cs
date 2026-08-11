namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments;

using Application.Models.Auth;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Constellation.Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetCurrentEnrolmentPeriods;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.PeriodSwitcher;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Presentation.Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Partners_Enrolments_Applications_View_Value)]
public class ApplicationsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public ApplicationsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Applications;
    [ViewData] public string PageTitle => "Enrolment Applications";

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
                Url = _linkGenerator.GetPathByPage("/Partner/Enrolments/Applications/Index", values: new { area = "Staff", PeriodId = p.Id })
            })
            .ToList();
    }

}