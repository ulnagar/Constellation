namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Periods;

using Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetEnrolmentPeriodById;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Shared;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[HasPermission(AuthPermission.Partners_Enrolments_Applications_Edit_Value)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Periods;
    [ViewData] public string PageTitle => "Enrolment Periods";

    [BindProperty(SupportsGet = true)]
    public EnrolmentPeriodId Id { get; set; } = EnrolmentPeriodId.Empty;

    [BindProperty]
    public string Label { get; set; }

    [BindProperty]
    public DateTimeOffset OpenAt { get; set; }

    [BindProperty]
    public DateTimeOffset ClosedAt { get; set; }

    [BindProperty]
    public Program Program { get; set; } = Program.Empty;

    [BindProperty]
    public PeriodStatus Status { get; set; }

    public SelectList ProgramList { get; set; }

    public async Task OnGet()
    {
        if (Id == EnrolmentPeriodId.Empty)
        {
            _logger
                .Information("Requested to load defaults for creation of new Enrolment Period by user {User}", _currentUserService.UserName);

            await PreparePage();
            return;
        }

        _logger
            .Information("Requested to load Enrolment Period for update by user {User}", _currentUserService.UserName);

        Result<EnrolmentPeriodResponse> period = await _mediator.Send(new GetEnrolmentPeriodByIdQuery(Id));

        if (period.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), period.Error, true)
                .Warning("Failed to load Enrolment Period for update by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                period.Error,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Periods/Index", values: new { area = "Staff" }));

            return;
        }

        Label = period.Value.Label;
        OpenAt = period.Value.OpenAt;
        ClosedAt = period.Value.ClosedAt;
        Program = period.Value.Program;
        Status = period.Value.Status;

        await PreparePage();
    }

    private async Task PreparePage()
    {
        ProgramList = new SelectList(
            Program.GetOptions,
            nameof(Program.Value),
            nameof(Program.Name),
            Program.Value);
    }
}