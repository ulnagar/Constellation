namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Applications;

using Application.Domains.EnrolmentContext.Applications.Models;
using Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationById;
using Application.Domains.EnrolmentContext.Applications.Queries.GetEnrolmentApplicationsByPeriod;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetAllEnrolmentPeriods;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Shared;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
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

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Applications;
    [ViewData] public string PageTitle => "Enrolment Applications";

    [BindProperty(SupportsGet = true)]
    public EnrolmentPeriodId PeriodId { get; set; } = EnrolmentPeriodId.Empty;

    public List<EnrolmentPeriodResponse> Periods { get; set; } = [];
    public List<EnrolmentApplicationResponse> Applications { get; set; } = [];

    public async Task OnGet()
    {
        await PreparePage();

        if (PeriodId == EnrolmentPeriodId.Empty)
        {
            if (Periods.Count is 0 or > 1)
                return;

            PeriodId = Periods.First().Id;
        }

        Result<List<EnrolmentApplicationResponse>> applications = await _mediator.Send(new GetEnrolmentApplicationsByPeriodQuery(PeriodId));
        
        if (applications.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(applications.Error);

            return;
        }

        Applications = applications.Value;
    }

    private async Task PreparePage()
    {
        Result<List<EnrolmentPeriodResponse>> periods = await _mediator.Send(new GetAllEnrolmentPeriodsQuery());

        if (periods.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(periods.Error);

            return;
        }

        Periods = periods.Value
            .OrderBy(entry => entry.OpenAt)
            .ToList();
    }
}