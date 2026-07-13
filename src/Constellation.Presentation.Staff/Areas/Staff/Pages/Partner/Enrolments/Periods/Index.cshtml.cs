namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Periods;

using Application.Common.PresentationModels;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[HasPermission(AuthPermission.Partners_Enrolments_Applications_View_Value)]
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
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Periods;
    [ViewData] public string PageTitle => "Enrolment Periods";

    [BindProperty(SupportsGet = true)] 
    public EnrolmentPeriodFilter Filter { get; set; } = EnrolmentPeriodFilter.Current;

    public List<EnrolmentPeriodResponse> Periods { get; set; } = [];

    public async Task OnGet()
    {
        Result<List<EnrolmentPeriodResponse>> periods = await _mediator.Send(new GetAllEnrolmentPeriodsQuery());

        if (periods.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(periods.Error);

            return;
        }

        Periods = FilterPeriods(periods.Value, Filter);
    }

    public enum EnrolmentPeriodFilter
    {
        All,
        Current,
        Archived
    }

    private static List<EnrolmentPeriodResponse> FilterPeriods(
        IEnumerable<EnrolmentPeriodResponse> periods,
        EnrolmentPeriodFilter filter)
    {
        return filter switch
        {
            EnrolmentPeriodFilter.All => periods.ToList(),

            EnrolmentPeriodFilter.Current => periods
                .Where(period => period.Status is
                    PeriodStatus.Open or
                    PeriodStatus.Suspended or
                    PeriodStatus.Scheduled)
                .ToList(),

            EnrolmentPeriodFilter.Archived => periods
                .Where(period => period.Status == PeriodStatus.Archived)
                .ToList(),

            _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
        };
    }
}