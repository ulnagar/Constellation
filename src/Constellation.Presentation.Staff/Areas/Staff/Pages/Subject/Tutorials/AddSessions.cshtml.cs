namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Tutorials;

using Application.Common.PresentationModels;
using Application.Domains.StaffMembers.Models;
using Application.Domains.StaffMembers.Queries.GetStaffForSelectionList;
using Application.Domains.Timetables.Periods.Queries.GetPeriodsForVisualSelection;
using Application.Domains.Tutorials.Commands.AddMultipleSessionsToTutorial;
using Application.Domains.Tutorials.Queries.GetSessionListForTutorial;
using Application.Domains.Tutorials.Queries.GetTutorialSummary;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class AddSessionsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AddSessionsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AddSessionsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Tutorials_Tutorials;
    [ViewData] public string PageTitle => "Bulk Add Sessions";

    [BindProperty(SupportsGet = true)]
    public TutorialId Id { get; set; }

    [BindProperty]
    public StaffId StaffId { get; set; }

    public string OfferingName { get; set; }

    [BindProperty] 
    public List<PeriodId> Periods { get; set; } = [];

    public List<PeriodVisualSelectResponse> ValidPeriods { get; set; } = [];
    public List<SessionListResponse> ExistingSessions { get; set; } = [];
    public List<StaffSelectionListResponse> StaffMembers { get; set; } = [];

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        AddMultipleSessionsToTutorialCommand command = new(Id, StaffId, Periods);

        _logger
            .ForContext(nameof(AddMultipleSessionsToTutorialCommand), command, true)
            .Information("Requested to add bulk Sessions to Tutorial by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to add bulk Sessions to Tutorial by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorials/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/Tutorials/Details", new { area = "Staff", Id = Id });
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve defaults for Bulk Add Sessions to Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<List<PeriodVisualSelectResponse>> periodRequest = await _mediator.Send(new GetPeriodsForVisualSelectionQuery());

        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), periodRequest.Error, true)
                .Warning("Failed to retrieve defaults for Bulk Add Sessions to Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                periodRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorial/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        ValidPeriods = periodRequest.Value;

        // Get current periods linked to Offering
        Result<List<SessionListResponse>> sessionRequest = await _mediator.Send(new GetSessionListForTutorialQuery(Id));

        if (sessionRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), sessionRequest.Error, true)
                .Warning("Failed to retrieve defaults for Bulk Add Sessions to Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                sessionRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorial/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        ExistingSessions = sessionRequest.Value;

        // Get current staff members
        var staffRequest = await _mediator.Send(new GetStaffForSelectionListQuery());

        if (staffRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), staffRequest.Error, true)
                .Warning("Failed to retrieve defaults for Bulk Add Sessions to Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                staffRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorial/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        StaffMembers = staffRequest.Value;

        // Get CourseName and OfferingName
        Result<TutorialSummaryResponse> tutorialRequest = await _mediator.Send(new GetTutorialSummaryQuery(Id));

        if (tutorialRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), tutorialRequest.Error, true)
                .Warning("Failed to retrieve defaults for Bulk Add Sessions to Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);
                
            ModalContent = ErrorDisplay.Create(
                tutorialRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorials/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        OfferingName = tutorialRequest.Value.Name;
    }
}