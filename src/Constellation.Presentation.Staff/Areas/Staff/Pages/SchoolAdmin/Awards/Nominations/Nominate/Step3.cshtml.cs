namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations.Nominate;

using Application.Awards.GetNominationPeriod;
using Application.Common.PresentationModels;
using Application.Courses.GetActiveCoursesList;
using Application.Courses.Models;
using Application.Models.Auth;
using Constellation.Application.Offerings.GetFilteredOfferingsForSelectionList;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Abstractions.Services;
using Core.Models.Identifiers;
using Core.Models.Subjects.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[Authorize(Policy = AuthPolicies.CanAddAwards)]
public class Step3Model : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public Step3Model(
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
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;
    [ViewData] public string PageTitle => "New Award Nomination";

    [BindProperty(SupportsGet = true)]
    public AwardNominationPeriodId PeriodId { get; set; }

    [BindProperty]
    [ModelBinder(typeof(FromValueBinder))]
    public AwardType Type { get; set; }

    [BindProperty]
    public CourseId CourseId { get; set; }
    public OfferingId? OfferingId { get; set; }


    public SelectList Courses { get; set; }
    public SelectList Offerings { get; set; }
    public SelectList AwardTypes { get; set; }


    public async Task<IActionResult> OnGet()
        => RedirectToPage("/SchoolAdmin/Awards/Nominations/Nominate/Step1", new { area = "Staff", PeriodId });


    public async Task<IActionResult> OnPost()
    {
        if (Type == AwardType.GalaxyMedal || Type == AwardType.PrincipalsAward || Type == AwardType.UniversalAchiever)
            return RedirectToPage("/SchoolAdmin/Awards/Nominations/Nominate/Step4", new { area = "Staff", PeriodId, Type });

        if (Type == AwardType.FirstInSubject)
            return RedirectToPage("/SchoolAdmin/Awards/Nominations/Nominate/Step4", new { area = "Staff", PeriodId, Type, CourseId });

        _logger.Information("Requested to create new Award Nomination by user {User}", _currentUserService.UserName);

        AwardTypes = new SelectList(AwardType.Options, "Value", "Value", Type);
        
        Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(PeriodId));

        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), periodRequest.Error, true)
                .Warning("Failed to create new Award Nomination by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                periodRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

            return Page();
        }

        Result<List<CourseSelectListItemResponse>> coursesRequest = await _mediator.Send(new GetActiveCoursesListQuery());

        if (coursesRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                coursesRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

            return Page();
        }

        Courses = new SelectList(coursesRequest.Value.Where(course => periodRequest.Value.IncludedGrades.Contains(course.Grade)), "Id", "DisplayName", CourseId.Value, "FacultyName");

        Result<List<OfferingForSelectionList>> offeringRequest = await _mediator.Send(new GetFilteredOfferingsForSelectionListQuery(new List<CourseId> { CourseId }));

        if (offeringRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                offeringRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

            return Page();
        }

        Offerings = new SelectList(offeringRequest.Value, "Id", "Name");

        return Page();
    }
}