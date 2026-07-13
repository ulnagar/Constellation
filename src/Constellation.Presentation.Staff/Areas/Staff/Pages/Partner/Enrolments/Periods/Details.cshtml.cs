namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Periods;

using Application.Common.PresentationModels;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.ReinstateEnrolmentPeriod;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.SuspendEnrolmentPeriod;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetEnrolmentPeriodById;
using Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Errors;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Attributes;
using Serilog;

[HasPermission(AuthPermission.Partners_Enrolments_Applications_View_Value)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Periods;
    [ViewData] public string PageTitle => "Enrolment Period Details";

    [BindProperty(SupportsGet = true)] 
    public EnrolmentPeriodId Id { get; set; } = EnrolmentPeriodId.Empty;

    public EnrolmentPeriodResponse Period { get; set; }

    public async Task OnGet()
    {
        if (Id == EnrolmentPeriodId.Empty)
        {
            ModalContent = ErrorDisplay.Create(
                EnrolmentPeriodErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Periods/Index", values: new { area = "Staff" }));

            return;
        }

        await PreparePage();
    }

    private async Task PreparePage()
    {
        Result<EnrolmentPeriodResponse> period = await _mediator.Send(new GetEnrolmentPeriodByIdQuery(Id));

        if (period.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                period.Error,
                _linkGenerator.GetPathByPage("/Partner/Enrolments/Periods/Index", values: new { area = "Staff" }));

            return;
        }

        Period = period.Value;
    }

    public async Task<IActionResult> OnPostSuspend(string reason)
    {
        SuspendEnrolmentPeriodCommand command = new(Id, reason);

        _logger
            .ForContext(nameof(SuspendEnrolmentPeriodCommand), command, true)
            .Information("Requested to suspend Enrolment Period by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(SuspendEnrolmentPeriodCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to suspend Enrolment Period by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReinstate()
    {
        ReinstateEnrolmentPeriodCommand command = new(Id);

        _logger
            .ForContext(nameof(ReinstateEnrolmentPeriodCommand), command, true)
            .Information("Requested to reinstate Enrolment Period by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(ReinstateEnrolmentPeriodCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to reinstate Enrolment Period by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage();
    }
}