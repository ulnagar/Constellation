namespace Constellation.Presentation.Schools.Areas.Schools.Pages.ScienceRolls;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Application.SciencePracs.GetLessonRollsForSchoolsPortal;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

[Authorize(Policy = AuthPolicies.IsSchoolContact)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public IndexModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor, 
        IServiceScopeFactory serviceFactory) 
        : base(httpContextAccessor, serviceFactory)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.ScienceRolls;

    [BindProperty(SupportsGet = true)] 
    public FilterEnum Filter { get; set; } = FilterEnum.Pending;

    public List<ScienceLessonRollSummary> Lessons { get; set; } = new();

    public async Task OnGet()
    {
        Result<List<ScienceLessonRollSummary>> lessonsRequest = await _mediator.Send(new GetLessonRollsForSchoolQuery(CurrentSchoolCode));

        if (lessonsRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(lessonsRequest.Error);

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