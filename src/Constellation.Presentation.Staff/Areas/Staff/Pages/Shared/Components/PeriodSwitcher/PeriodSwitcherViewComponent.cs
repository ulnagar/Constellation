namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.PeriodSwitcher;

using Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetCurrentEnrolmentPeriods;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;

public sealed class PeriodSwitcherViewComponent : ViewComponent
{
    private readonly IMediator _mediator;

    public PeriodSwitcherViewComponent(
        IMediator mediator) =>
        _mediator = mediator;

    public async Task<IViewComponentResult> InvokeAsync()
    {
        Result<List<EnrolmentPeriodResponse>> periodsResult = await _mediator.Send(new GetAllEnrolmentPeriodsQuery());
        string? currentPeriodId = ViewContext.RouteData.Values["periodId"]?.ToString();

        RouteValueDictionary baseValues = new RouteValueDictionary(ViewContext.RouteData.Values);
        baseValues.Remove("page");
        baseValues.Remove("handler");

        foreach (KeyValuePair<string, StringValues> kvp in ViewContext.HttpContext.Request.Query)
            baseValues[kvp.Key] = kvp.Value.ToString();

        List<PeriodSwitcherOption> options = periodsResult.Value
            .Select(p => new PeriodSwitcherOption { 
                PeriodId = p.Id,
                Label = p.Label,
                IsCurrent = p.Status <= PeriodStatus.Open,
                CurrentlySelected = p.Id.ToString() == currentPeriodId,
                Url = BuildSwitchUrl(baseValues, p.Id)
            })
            .ToList();

        return View(new PeriodSwitcherViewModel(options));
    }

    private string BuildSwitchUrl(RouteValueDictionary baseValues, EnrolmentPeriodId targetPeriodId)
    {
        RouteValueDictionary values = new RouteValueDictionary(baseValues) { ["periodId"] = targetPeriodId };
        return Url.Page(pageName: null, values: values) ?? Url.Page("/Staff/Partner/Enrolments/Periods/Index");
    }
}

