namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations.Nominate;

using Application.Common.PresentationModels;
using Application.Models.Auth;
using Constellation.Application.Domains.Courses.Models;
using Constellation.Application.Domains.Courses.Queries.GetActiveCoursesList;
using Constellation.Application.Domains.MeritAwards.Nominations.Commands.CreateAwardNomination;
using Constellation.Application.Domains.MeritAwards.Nominations.Queries.GetNominationPeriod;
using Constellation.Application.Domains.Offerings.Queries.GetFilteredOfferingsForSelectionList;
using Constellation.Application.Domains.Students.Queries.GetFilteredStudentsForSelectionList;
using Constellation.Core.Enums;
using Core.Abstractions.Services;
using Core.Models.Identifiers;
using Core.Models.Offerings.Identifiers;
using Core.Models.Students.Identifiers;
using Core.Models.Subjects.Identifiers;
using Core.Shared;
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
public class Step4Model : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public Step4Model(
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

    [BindProperty]
    public CourseId CourseId { get; set; } = CourseId.Empty;

    [BindProperty]
    public OfferingId OfferingId { get; set; } = OfferingId.Empty;

    [BindProperty] 
    public StudentId StudentId { get; set; } = StudentId.Empty;

    public List<StudentForSelectionList> StudentsList { get; set; }
    public SelectList Courses { get; set; }
    public SelectList Offerings { get; set; }
    public SelectList AwardTypes { get; set; }


    public async Task<IActionResult> OnGet()
        => RedirectToPage("/SchoolAdmin/Awards/Nominations/Nominate/Step1", new { area = "Staff", PeriodId });
    
    public async Task<IActionResult> OnPost()
    {
        if (StudentId == StudentId.Empty)
        {
            return await PreparePage();
        }

        CreateAwardNominationCommand command = new(
            PeriodId,
            Type,
            CourseId,
            OfferingId,
            StudentId);

        Result response = await _mediator.Send(command);

        if (response.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                response.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Nominate/Step1", values: new { area = "Staff", PeriodId }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Awards/Nominations/Details", new { area = "Staff", PeriodId = PeriodId });
    }


    private async Task<IActionResult> PreparePage()
    {
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

        if (CourseId != CourseId.Empty)
        {
            Result<List<CourseSelectListItemResponse>> coursesRequest = await _mediator.Send(new GetActiveCoursesListQuery());

            if (coursesRequest.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(
                    coursesRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return Page();
            }

            Courses = new SelectList(coursesRequest.Value.Where(course => periodRequest.Value.IncludedGrades.Contains(course.Grade)), "Id", "DisplayName", CourseId.Value, "FacultyName");
        }

        if (OfferingId != OfferingId.Empty)
        {
            Result<List<OfferingForSelectionList>> offeringRequest = await _mediator.Send(new GetFilteredOfferingsForSelectionListQuery(new List<CourseId> { CourseId }));

            if (offeringRequest.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(
                    offeringRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details",
                        values: new { area = "Staff", PeriodId = PeriodId }));

                return Page();
            }

            Offerings = new SelectList(offeringRequest.Value, "Id", "Name");
        }

        if (OfferingId != OfferingId.Empty)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(new List<Grade>(), new List<OfferingId> { OfferingId }, new List<CourseId>()));

            if (studentsRequest.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return Page();
            }

            StudentsList = studentsRequest.Value.OrderBy(entry => entry.StudentName.SortOrder).ToList();
        }
        else if (CourseId != CourseId.Empty)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(new List<Grade>(), new List<OfferingId>(), new List<CourseId> { CourseId }));

            if (studentsRequest.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return Page();
            }

            StudentsList = studentsRequest.Value.OrderBy(entry => entry.StudentName.SortOrder).ToList();
        }
        else
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(periodRequest.Value.IncludedGrades, new List<OfferingId>(), new List<CourseId>()));

            if (studentsRequest.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return Page();
            }

            StudentsList = studentsRequest.Value.OrderBy(entry => entry.StudentName.SortOrder).ToList();
        }

        return Page();
    }
}