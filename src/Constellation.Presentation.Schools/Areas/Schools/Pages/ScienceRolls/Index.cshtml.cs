namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ScienceRolls;

using Application.Common.PresentationModels;
using Application.Domains.SciencePracs.Queries.GetLessonRollsForSchoolsPortal;
using Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Logging;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Collections.Generic;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
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
        ILogger logger,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForContext(LogDefaults.Application, LogDefaults.SchoolsPortal);
    }

    [ViewData] public string ActivePage => Models.ActivePage.ScienceRolls;

    [BindProperty(SupportsGet = true)] 
    public FilterEnum Filter { get; set; } = FilterEnum.Pending;

    public List<ScienceLessonRollSummary> Lessons { get; set; } = new();

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve science lesson list by user {user} for school {school}", _currentUserService.UserName, CurrentSchoolCode);

        Result<List<ScienceLessonRollSummary>> lessonsRequest = await _mediator.Send(new GetLessonRollsForSchoolQuery(CurrentSchoolCode));

        if (lessonsRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(lessonsRequest.Error);

            return;
        }

        Lessons = Filter switch
        {
            FilterEnum.Pending => lessonsRequest.Value.Where(lesson => !lesson.IsSubmitted).ToList(),
            FilterEnum.Overdue => lessonsRequest.Value.Where(lesson => lesson.IsOverdue).ToList(),
            FilterEnum.Complete => lessonsRequest.Value.Where(lesson => lesson.IsSubmitted).ToList(),
            FilterEnum.All => lessonsRequest.Value,
            _ => lessonsRequest.Value
        };

        Lessons = Lessons
            .OrderBy(lesson => lesson.Grade)
            .ThenBy(lesson => lesson.LessonCourseName)
            .ThenBy(lesson => lesson.LessonDueDate)
            .ToList();
    }

    public enum FilterEnum
    {
        All,
        Pending,
        Overdue,
        Complete
    }
}