namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Periods.Edval;

using Application.Common.PresentationModels;
using Application.Domains.Edval.Commands.IgnoreEdvalDifference;
using Application.Domains.Edval.Commands.RefreshClassDifferences;
using Application.Domains.Edval.Commands.RefreshDifferences;
using Application.Domains.Edval.Commands.RefreshStudentDifferences;
using Application.Domains.Edval.Commands.RefreshTeacherDifferences;
using Application.Domains.Edval.Commands.RefreshTimetableDifferences;
using Application.Domains.Edval.Commands.RemoveIgnoreRuleForDifference;
using Application.Domains.Edval.Queries.GetEdvalDifferences;
using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.Edval;
using Core.Models.Edval.Enums;
using Core.Models.Edval.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.CanManageAbsences)]
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
            .ForContext<IndexModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Periods_Edval;
    [ViewData] public string PageTitle => "Edval Differences";

    [BindProperty(SupportsGet = true)]
    public EdvalFilter Filter { get; set; } = EdvalFilter.All;

    public List<Difference> Differences { get; set; } = new();

    public async Task OnGet() => await PreparePage();

    public async Task OnGetRefresh()
    {
        _logger
            .Information("Requested to refresh Edval Differences for user {User}", _currentUserService.UserName);

        Result result = Filter switch
        {
            EdvalFilter.Class => await _mediator.Send(new RefreshClassDifferencesCommand()),
            EdvalFilter.ClassMembership => await _mediator.Send(new RefreshClassDifferencesCommand()),
            EdvalFilter.Student => await _mediator.Send(new RefreshStudentDifferencesCommand()),
            EdvalFilter.Teacher => await _mediator.Send(new RefreshTeacherDifferencesCommand()),
            EdvalFilter.Timetable => await _mediator.Send(new RefreshTimetableDifferencesCommand()),
            _ => await _mediator.Send(new RefreshDifferencesCommand())
        };

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to refresh Edval Differences for user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                null);
        }
        
        await PreparePage();
    }

    public async Task<IActionResult> OnGetIgnore(DifferenceId differenceId)
    {
        _logger
            .ForContext(nameof(DifferenceId), differenceId)
            .Information("Requested to create Ignore record for Edval Difference by user {User}", _currentUserService.UserName);

        Result attempt = await _mediator.Send(new IgnoreEdvalDifferenceCommand(differenceId));

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .ForContext(nameof(DifferenceId), differenceId)
                .Warning("Failed to create Ignore record for Edval Difference by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                attempt.Error,
                _linkGenerator.GetPathByPage("/Subject/Periods/Edval/Index", values: new { area = "Staff", Filter }));

            return Page();
        }

        return RedirectToPage(new { Filter });
    }

    public async Task<IActionResult> OnGetRegard(DifferenceId differenceId)
    {
        _logger
            .ForContext(nameof(DifferenceId), differenceId)
            .Information("Requested to delete Ignore record for Edval Difference by user {User}", _currentUserService.UserName);

        Result attempt = await _mediator.Send(new RemoveIgnoreRuleForDifferenceCommand(differenceId));

        if (attempt.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), attempt.Error, true)
                .ForContext(nameof(DifferenceId), differenceId)
                .Warning("Failed to delete Ignore record for Edval Difference by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                attempt.Error,
                _linkGenerator.GetPathByPage("/Subject/Periods/Edval/Index", values: new { area = "Staff", Filter }));

            return Page();
        }

        return RedirectToPage(new { Filter });
    }

    private async Task PreparePage()
    {
        _logger
            .Information("Requested to retrieve list of Edval Differences for user {User}", _currentUserService.UserName);

        Result<List<Difference>> differences = await _mediator.Send(new GetEdvalDifferencesQuery());

        if (differences.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), differences.Error, true)
                .Warning("Failed to retrieve list of Edval Differences for user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                differences.Error,
                _linkGenerator.GetPathByPage("Dashboard", values: new { area = "Staff" }));

            return;
        }

        Differences = Filter switch
        {
            EdvalFilter.Class => differences.Value.Where(difference => !difference.Ignored && difference.Type.Equals(EdvalDifferenceType.EdvalClass)).ToList(),
            EdvalFilter.ClassMembership => differences.Value.Where(difference => !difference.Ignored && difference.Type.Equals(EdvalDifferenceType.EdvalClassMembership)).ToList(),
            EdvalFilter.Student => differences.Value.Where(difference => !difference.Ignored && difference.Type.Equals(EdvalDifferenceType.EdvalStudent)).ToList(),
            EdvalFilter.Teacher => differences.Value.Where(difference => !difference.Ignored && difference.Type.Equals(EdvalDifferenceType.EdvalTeacher)).ToList(),
            EdvalFilter.Timetable => differences.Value.Where(difference => !difference.Ignored && difference.Type.Equals(EdvalDifferenceType.EdvalTimetable)).ToList(),
            EdvalFilter.Ignored => differences.Value.Where(difference => difference.Ignored).ToList(),
            _ => differences.Value.Where(difference => !difference.Ignored).ToList()
        };
    }

    public enum EdvalFilter
    {
        All,
        Class,
        ClassMembership,
        Student,
        Teacher,
        Timetable,
        Ignored
    }
}