namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.SciencePracs.Lessons;

using Application.Common.PresentationModels;
using Application.Domains.SciencePracs.Queries.GetLessonsFromCurrentYear;
using Constellation.Application.Models.Auth;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService, 
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_SciencePracs_Lessons;
    [ViewData] public string PageTitle => "Lessons List";

    [BindProperty(SupportsGet = true)]
    public FilterDto Filter { get; set; } = FilterDto.Current;

    public List<LessonSummaryResponse> Lessons { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve list of Lessons by user {User}", _currentUserService.UserName);

        Result<List<LessonSummaryResponse>> lessonRequest = await _mediator.Send(new GetLessonsFromCurrentYearQuery());

        if (lessonRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), lessonRequest.Error, true)
                .Warning("Failed to retrieve list of Lessons by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(lessonRequest.Error);

            return;
        }

        Lessons = Filter switch
        {
            FilterDto.All => lessonRequest.Value,
            FilterDto.Overdue => lessonRequest.Value.Where(lesson => lesson.Overdue).ToList(),
            FilterDto.Current => lessonRequest.Value.Where(lesson => lesson.Overdue || lesson.DueDate >= DateOnly.FromDateTime(DateTime.Today)).ToList(),
            FilterDto.Future => lessonRequest.Value.Where(lesson => lesson.DueDate >= DateOnly.FromDateTime(DateTime.Today)).ToList(),
            _ => lessonRequest.Value
        };

        Lessons = Lessons.OrderBy(lesson => lesson.DueDate).ToList();
    }

    public enum FilterDto
    {
        All,
        Overdue,
        Current,
        Future
    }
}
