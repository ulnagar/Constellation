namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations.Nominate;

using Application.Awards.CreateAwardNomination;
using Application.Awards.GetNominationPeriod;
using Application.Common.PresentationModels;
using Application.Courses.GetActiveCoursesList;
using Application.Courses.Models;
using Application.Offerings.GetFilteredOfferingsForSelectionList;
using Constellation.Application.Students.GetFilteredStudentsForSelectionList;
using Constellation.Core.Enums;
using Core.Abstractions.Services;
using Core.Models.Identifiers;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;
using Core.Shared;
using Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

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
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;
    [ViewData] public string PageTitle => "New Award Nomination";

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public AwardNominationPeriodId PeriodId { get; set; }

    [BindProperty]
    [ModelBinder(typeof(ValueObjectBinder))]
    public AwardType Type { get; set; }

    [BindProperty]
    [ModelBinder(typeof(StrongIdBinder))]
    public CourseId? CourseId { get; set; }

    [BindProperty]
    [ModelBinder(typeof(StrongIdBinder))]
    public OfferingId? OfferingId { get; set; }

    [BindProperty]
    public string? StudentId { get; set; }

    public List<StudentForSelectionList> StudentsList { get; set; }
    public SelectList Courses { get; set; }
    public SelectList Offerings { get; set; }
    public SelectList AwardTypes { get; set; }


    public async Task<IActionResult> OnGet()
        => RedirectToPage("/SchoolAdmin/Awards/Nominations/Nominate/Step1", new { area = "Staff", PeriodId });
    
    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(StudentId))
        {
            return await PreparePage();
        }

        CreateAwardNominationCommand command = new(
            PeriodId,
            Type,
            CourseId,
            OfferingId ?? Core.Models.Offerings.Identifiers.OfferingId.Empty,
            StudentId);

        Result response = await _mediator.Send(command);

        if (response.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                response.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Nominate/Step1", values: new { area = "Staff", PeriodId }));

            return Page();
        }

        return RedirectToPage("/SchoolAdmin/Awards/Nominations/Details", new { area = "SchoolAdmin", PeriodId = PeriodId });
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

            ModalContent = new ErrorDisplay(
                periodRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

            return Page();
        }

        if (CourseId is not null)
        {
            Result<List<CourseSelectListItemResponse>> coursesRequest = await _mediator.Send(new GetActiveCoursesListQuery());

            if (coursesRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    coursesRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return Page();
            }

            Courses = new SelectList(coursesRequest.Value.Where(course => periodRequest.Value.IncludedGrades.Contains(course.Grade)), "Id", "DisplayName", CourseId.Value, "FacultyName");
        }

        if (OfferingId is not null)
        {
            Result<List<OfferingForSelectionList>> offeringRequest =
                await _mediator.Send(new GetFilteredOfferingsForSelectionListQuery(new List<CourseId> { CourseId }));

            if (offeringRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    offeringRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details",
                        values: new { area = "Staff", PeriodId = PeriodId }));

                return Page();
            }

            Offerings = new SelectList(offeringRequest.Value, "Id", "Name");
        }

        if (OfferingId is not null)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(new List<Grade>(), new List<OfferingId> { OfferingId.Value }, new List<CourseId>()));

            if (studentsRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return Page();
            }

            StudentsList = studentsRequest.Value;
        }
        else if (CourseId != CourseId.Empty)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(new List<Grade>(), new List<OfferingId>(), new List<CourseId> { CourseId }));

            if (studentsRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return Page();
            }

            StudentsList = studentsRequest.Value;
        }
        else
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(periodRequest.Value.IncludedGrades, new List<OfferingId>(), new List<CourseId>()));

            if (studentsRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return Page();
            }

            StudentsList = studentsRequest.Value;
        }

        return Page();
    }
}