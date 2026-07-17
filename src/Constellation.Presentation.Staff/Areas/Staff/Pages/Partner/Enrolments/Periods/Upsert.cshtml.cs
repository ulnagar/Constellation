namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Enrolments.Periods;

using Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.CreateEnrolmentPeriod;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Commands.UpdateEnrolmentPeriod;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Models;
using Application.Domains.EnrolmentContext.EnrolmentPeriods.Queries.GetEnrolmentPeriodById;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;
using Constellation.Core.Models.EnrolmentContext.Offer.Enums;
using Constellation.Core.Shared;
using Core.Abstractions.Clock;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.EnrolmentContext.EnrolmentPeriod.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Attributes;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[HasPermission(AuthPermission.Partners_Enrolments_Applications_Edit_Value)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IDateTimeProvider _dateTime;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IDateTimeProvider dateTime,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Enrolments_Periods;
    [ViewData] public string PageTitle => "Enrolment Periods";

    [BindProperty(SupportsGet = true)]
    public EnrolmentPeriodId Id { get; set; } = EnrolmentPeriodId.Empty;

    [BindProperty]
    public string Label { get; set; }

    [BindProperty]
    public DateTime OpenAt { get; set; }

    [BindProperty]
    public DateTime ClosedAt { get; set; }

    [BindProperty]
    public Program Program { get; set; } = Program.Empty;
    
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
        OpenAt = TimeZoneInfo.ConvertTime(period.Value.OpenAt, _dateTime.SydneyTZ).DateTime;
        ClosedAt = TimeZoneInfo.ConvertTime(period.Value.ClosedAt, _dateTime.SydneyTZ).DateTime;
        Program = period.Value.Program;

        await PreparePage();
    }

    public async Task<IActionResult> OnPostCreate()
    {
        ValidateForm();

        if (!ModelState.IsValid)
        {
            _logger
                .Warning("Failed to validate Enrolment Period create form by user {User}", _currentUserService.UserName);

            await PreparePage();
            return Page();
        }

        DateTime openAtUnspecified = DateTime.SpecifyKind(OpenAt, DateTimeKind.Unspecified);
        DateTimeOffset openAtOffset = new(openAtUnspecified, _dateTime.SydneyTZ.GetUtcOffset(openAtUnspecified));

        DateTime closedAtUnspecified = DateTime.SpecifyKind(ClosedAt, DateTimeKind.Unspecified);
        DateTimeOffset closedAtOffset = new(closedAtUnspecified, _dateTime.SydneyTZ.GetUtcOffset(closedAtUnspecified));

        CreateEnrolmentPeriodCommand command = new(
            Label,
            openAtOffset,
            closedAtOffset,
            Program);

        _logger
            .ForContext(nameof(CreateEnrolmentPeriodCommand), command, true)
            .Information("Requested to create Enrolment Period by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateEnrolmentPeriodCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to create Enrolment Period by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage("/Partner/Enrolments/Periods/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        ValidateForm();

        if (!ModelState.IsValid)
        {
            _logger
                .Warning("Failed to validate Enrolment Period update form by user {User}", _currentUserService.UserName);

            await PreparePage();
            return Page();
        }

        DateTime openAtUnspecified = DateTime.SpecifyKind(OpenAt, DateTimeKind.Unspecified);
        DateTimeOffset openAtOffset = new(openAtUnspecified, _dateTime.SydneyTZ.GetUtcOffset(openAtUnspecified));

        DateTime closedAtUnspecified = DateTime.SpecifyKind(ClosedAt, DateTimeKind.Unspecified);
        DateTimeOffset closedAtOffset = new(closedAtUnspecified, _dateTime.SydneyTZ.GetUtcOffset(closedAtUnspecified));

        UpdateEnrolmentPeriodCommand command = new(
            Id,
            Label,
            openAtOffset,
            closedAtOffset,
            Program);

        _logger
            .ForContext(nameof(UpdateEnrolmentPeriodCommand), command, true)
            .Information("Requested to update Enrolment Period by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(UpdateEnrolmentPeriodCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to update Enrolment Period by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(result.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage("/Partner/Enrolments/Periods/Index", new { area = "Staff" });
    }
    
    private async Task PreparePage()
    {
        ProgramList = new SelectList(
            Program.GetOptions,
            nameof(Program.Value),
            nameof(Program.Name),
            Program.Value);
    }

    private void ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(Label))
            ModelState.AddModelError(nameof(Label), "Label is required");

        if (Program == Program.Empty)
            ModelState.AddModelError(nameof(Program), "Program is required");

        DateTime openAtUnspecified = DateTime.SpecifyKind(OpenAt, DateTimeKind.Unspecified);
        DateTimeOffset openAtOffset = new(openAtUnspecified, _dateTime.SydneyTZ.GetUtcOffset(openAtUnspecified));

        DateTime closedAtUnspecified = DateTime.SpecifyKind(ClosedAt, DateTimeKind.Unspecified);
        DateTimeOffset closedAtOffset = new(closedAtUnspecified, _dateTime.SydneyTZ.GetUtcOffset(closedAtUnspecified));

        Result isValidDates = EnrolmentPeriod.ValidatePeriod(openAtOffset, closedAtOffset, _dateTime.Now);

        if (isValidDates.IsFailure)
        {
            ModelState.AddModelError(nameof(OpenAt), isValidDates.Error.Message);
            ModelState.AddModelError(nameof(ClosedAt), isValidDates.Error.Message);
        }
    }
}