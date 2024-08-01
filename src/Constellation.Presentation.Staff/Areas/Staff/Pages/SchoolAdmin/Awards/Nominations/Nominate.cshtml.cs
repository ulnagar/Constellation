namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations;

using Application.Common.PresentationModels;
using Application.Courses.GetActiveCoursesList;
using Constellation.Application.Awards.CreateAwardNomination;
using Constellation.Application.Awards.GetAllNominationPeriods;
using Constellation.Application.Awards.GetNominationPeriod;
using Constellation.Application.Courses.Models;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetFilteredOfferingsForSelectionList;
using Constellation.Application.Students.GetFilteredStudentsForSelectionList;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.ModelBinders;

[Authorize(Policy = AuthPolicies.CanAddAwards)]
public class NominateModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public NominateModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.SchoolAdmin_Awards_Nominations;

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(StrongIdBinder))]
    public AwardNominationPeriodId PeriodId { get; set; }

    public NominationPeriodResponse Period { get; set; }

    [BindProperty]
    public string? StudentId { get; set; }
    [BindProperty]
    public string? Type { get; set; }
    [BindProperty]
    [ModelBinder(typeof(StrongIdBinder))]
    public CourseId? CourseId { get; set; }
    [BindProperty]
    [ModelBinder(typeof(StrongIdBinder))]
    public OfferingId? OfferingId { get; set; }
    [BindProperty]
    public Phase CurrentStep { get; set; }
    [BindProperty]
    public List<Phase> PreviousSteps { get; set; } = new();

    public List<StudentForSelectionList> StudentsList { get; set; }
    public SelectList Courses { get; set; }
    public SelectList Offerings { get; set; }

    public async Task OnGet()
    {
        await PreparePage();
    }

    public async Task<IActionResult> OnPost()
    {
        ValidateEntries();

        if (!ModelState.IsValid) 
        {
            await PreparePage();

            return Page();
        }

        AwardType? awardType = AwardType.FromValue(Type);

        if (awardType is null)
        {
            await PreparePage();

            return Page();
        }

        if (CurrentStep == Phase.AwardSelection &&
            (awardType.Equals(AwardType.FirstInSubject) ||
            awardType.Equals(AwardType.AcademicExcellence) ||
            awardType.Equals(AwardType.AcademicExcellenceMathematics) ||
            awardType.Equals(AwardType.AcademicExcellenceScienceTechnology) ||
            awardType.Equals(AwardType.AcademicAchievement) ||
            awardType.Equals(AwardType.AcademicAchievementMathematics) ||
            awardType.Equals(AwardType.AcademicAchievementScienceTechnology)))
        {
            PreviousSteps.Add(Phase.AwardSelection);
            CurrentStep = Phase.CourseSelection;

            await PreparePage();
            return Page();
        }

        if (CurrentStep == Phase.AwardSelection &&
            (awardType.Equals(AwardType.GalaxyMedal) ||
            awardType.Equals(AwardType.PrincipalsAward) ||
            awardType.Equals(AwardType.UniversalAchiever)))
        {
            PreviousSteps.Add(Phase.AwardSelection);
            CurrentStep = Phase.StudentSelection;

            await PreparePage();
            return Page();
        }

        if (CurrentStep == Phase.CourseSelection &&
            (awardType.Equals(AwardType.AcademicExcellence) ||
            awardType.Equals(AwardType.AcademicExcellenceMathematics) ||
            awardType.Equals(AwardType.AcademicExcellenceScienceTechnology) ||
            awardType.Equals(AwardType.AcademicAchievement) ||
            awardType.Equals(AwardType.AcademicAchievementMathematics) ||
            awardType.Equals(AwardType.AcademicAchievementScienceTechnology)))
        {
            PreviousSteps.Add(Phase.CourseSelection);
            CurrentStep = Phase.OfferingSelection;

            await PreparePage();
            return Page();
        }

        if (CurrentStep == Phase.CourseSelection && 
            awardType.Equals(AwardType.FirstInSubject))
        {
            PreviousSteps.Add(Phase.CourseSelection);
            CurrentStep = Phase.StudentSelection;

            await PreparePage();
            return Page();
        }

        if (CurrentStep == Phase.OfferingSelection)
        {
            PreviousSteps.Add(Phase.OfferingSelection);
            CurrentStep = Phase.StudentSelection;

            await PreparePage();
            return Page();
        }

        if (CourseId is null || CourseId == CourseId.Empty || OfferingId is null || OfferingId == Core.Models.Offerings.Identifiers.OfferingId.Empty)
        {
            await PreparePage();

            return Page();
        }

        CreateAwardNominationCommand command = new(
            PeriodId,
            AwardType.FromValue(Type),
            CourseId,
            OfferingId.Value,
            StudentId);

        Result response = await _mediator.Send(command);

        if (response.IsFailure)
        {
            ModalContent = new ErrorDisplay(response.Error);

            await PreparePage();
            return Page();
        }

        return RedirectToPage("/Awards/Nominations/Details", new { area = "SchoolAdmin", PeriodId = PeriodId });
    }

    private async Task PreparePage()
    {
        Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(PeriodId));

        if (periodRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                periodRequest.Error,
                _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

            return;
        }

        if (CurrentStep == Phase.AwardSelection)
            return;

        if (CurrentStep == Phase.CourseSelection)
        {
            Result<List<CourseSelectListItemResponse>> coursesRequest = await _mediator.Send(new GetActiveCoursesListQuery());

            if (coursesRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    coursesRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return;
            }

            Courses = new SelectList(coursesRequest.Value.Where(course => periodRequest.Value.IncludedGrades.Contains(course.Grade)), "Id", "DisplayName", null, "FacultyName");
        }

        if (PreviousSteps.Contains(Phase.CourseSelection))
        {
            Result<List<CourseSelectListItemResponse>> coursesRequest = await _mediator.Send(new GetActiveCoursesListQuery());

            if (coursesRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    coursesRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return;
            }

            Courses = new SelectList(coursesRequest.Value.Where(course => periodRequest.Value.IncludedGrades.Contains(course.Grade)), "Id", "DisplayName", CourseId.Value, "FacultyName");
        }

        if (CurrentStep == Phase.OfferingSelection)
        {
            Result<List<OfferingForSelectionList>> offeringRequest = await _mediator.Send(new GetFilteredOfferingsForSelectionListQuery(new List<CourseId> { CourseId }));

            if (offeringRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    offeringRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return;
            }

            Offerings = new SelectList(offeringRequest.Value, "Id", "Name");
        }

        if (PreviousSteps.Contains(Phase.OfferingSelection))
        {
            Result<List<OfferingForSelectionList>> offeringRequest = await _mediator.Send(new GetFilteredOfferingsForSelectionListQuery(new List<CourseId> { CourseId }));

            if (offeringRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    offeringRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return;
            }

            Offerings = new SelectList(offeringRequest.Value, "Id", "Name", OfferingId);
        }

        if (CurrentStep == Phase.StudentSelection && OfferingId is not null && OfferingId != Core.Models.Offerings.Identifiers.OfferingId.Empty)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(new List<Grade>(), new List<OfferingId> { OfferingId.Value }, new List<CourseId>()));

            if (studentsRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return;
            }

            StudentsList = studentsRequest.Value;
        }
        else if (CurrentStep == Phase.StudentSelection && CourseId != CourseId.Empty)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(new List<Grade>(), new List<OfferingId>(), new List<CourseId> { CourseId } ));

            if (studentsRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return;
            }

            StudentsList = studentsRequest.Value;
        }
        else if (CurrentStep == Phase.StudentSelection)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(periodRequest.Value.IncludedGrades, new List<OfferingId>(), new List<CourseId>()));

            if (studentsRequest.IsFailure)
            {
                ModalContent = new ErrorDisplay(
                    studentsRequest.Error,
                    _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId }));

                return;
            }

            StudentsList = studentsRequest.Value;
        }
    }

    private void ValidateEntries()
    {
        if (CurrentStep == Phase.AwardSelection || PreviousSteps.Contains(Phase.AwardSelection)) 
        {
            if (Type is null)
            {
                ModelState.AddModelError("AwardType", "You must select an award type");
            }

            if (Type != AwardType.FirstInSubject.Value &&
                Type != AwardType.AcademicExcellence.Value &&
                Type != AwardType.AcademicExcellenceMathematics.Value &&
                Type != AwardType.AcademicExcellenceScienceTechnology.Value &&
                Type != AwardType.AcademicAchievement.Value &&
                Type != AwardType.AcademicAchievementMathematics.Value &&
                Type != AwardType.AcademicAchievementScienceTechnology.Value &&
                Type != AwardType.PrincipalsAward.Value &&
                Type != AwardType.GalaxyMedal.Value &&
                Type != AwardType.UniversalAchiever.Value)
            {
                ModelState.AddModelError("AwardType", "You must select a valid award type");
            }
        }

        if (CurrentStep == Phase.CourseSelection || PreviousSteps.Contains(Phase.CourseSelection))
        {
            if ((Type == AwardType.FirstInSubject.Value ||
                Type == AwardType.AcademicExcellence.Value ||
                Type != AwardType.AcademicExcellenceMathematics.Value ||
                Type != AwardType.AcademicExcellenceScienceTechnology.Value ||
                Type == AwardType.AcademicAchievement.Value ||
                Type != AwardType.AcademicAchievementMathematics.Value ||
                Type != AwardType.AcademicAchievementScienceTechnology.Value) &&
                CourseId == CourseId.Empty)
            {
                ModelState.AddModelError("CourseId", "You must select a valid course");
            }
        }

        if (CurrentStep == Phase.OfferingSelection || PreviousSteps.Contains(Phase.OfferingSelection))
        {
            if ((Type == AwardType.FirstInSubject.Value ||
                 Type == AwardType.AcademicExcellence.Value ||
                 Type != AwardType.AcademicExcellenceMathematics.Value ||
                 Type != AwardType.AcademicExcellenceScienceTechnology.Value ||
                 Type == AwardType.AcademicAchievement.Value ||
                 Type != AwardType.AcademicAchievementMathematics.Value ||
                 Type != AwardType.AcademicAchievementScienceTechnology.Value) &&
                OfferingId == Core.Models.Offerings.Identifiers.OfferingId.Empty)
            {
                ModelState.AddModelError("OfferingId", "You must select a valid class");
            }
        }

        if (CurrentStep == Phase.StudentSelection)
        {
            if (StudentId is null)
            {
                ModelState.AddModelError("StudentId", "You must select a student");
            }
        }
    }

    public enum Phase
    {
        AwardSelection,
        CourseSelection,
        OfferingSelection,
        StudentSelection
    }
}
