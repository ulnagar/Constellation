namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations.Nominate;

using Application.Domains.Courses.Queries.GetActiveCoursesList;
using Application.Models.Auth;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Courses.Models;
using Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetNominationPeriod;
using Constellation.Core.Models.Awards.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Models.Identifiers;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.CanAddAwards)]
public class Step2Model : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public Step2Model(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<Step1Model>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;
    [ViewData] public string PageTitle => "New Award Nomination";

    [BindProperty(SupportsGet = true)]
    public AwardNominationPeriodId PeriodId { get; set; }

    [BindProperty]
    [ModelBinder(typeof(FromValueBinder))]
    public AwardType Type { get; set; }

    public CourseId? CourseId { get; set; }

    public SelectList Courses { get; set; }
    public SelectList AwardTypes { get; set; }

    public async Task<IActionResult> OnGet() 
        => RedirectToPage("/SchoolAdmin/Awards/Nominations/Nominate/Step1", new { area = "Staff", PeriodId });

    public async Task<IActionResult> OnPost()
    {
        if (Type == AwardType.GalaxyMedal || Type == AwardType.PrincipalsAward || Type == AwardType.UniversalAchiever)
            return RedirectToPage("/SchoolAdmin/Awards/Nominations/Nominate/Step4", new { area = "Staff", PeriodId, Type });

        _logger.Information("Requested to create new Award Nomination by user {User}", _currentUserService.UserName);

        AwardTypes = new SelectList(AwardType.Options, "Value", "Value", Type);

        Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(PeriodId));

        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), periodRequest.Error, true)
                .Warning("Failed to create new Award Nomination by user {User}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                periodRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

            return Page();
        }

        Result<List<CourseSelectListItemResponse>> coursesRequest = await _mediator.Send(new GetActiveCoursesListQuery());

        if (coursesRequest.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                coursesRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

            return Page();
        }

        Courses = new SelectList(coursesRequest.Value.Where(course => periodRequest.Value.IncludedGrades.Contains(course.Grade)), "Id", "DisplayName", null, "FacultyName");

        return Page();
    }
}