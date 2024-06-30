namespace Constellation.Presentation.Staff.Areas.Staff.Pages.SchoolAdmin.Awards.Nominations;

using Constellation.Application.Awards.CreateAwardNomination;
using Constellation.Application.Awards.GetAllNominationPeriods;
using Constellation.Application.Awards.GetNominationPeriod;
using Constellation.Application.Courses.GetCoursesForSelectionList;
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
    public Guid PeriodId { get; set; }

    public NominationPeriodResponse Period { get; set; }

    [BindProperty]
    public string StudentId { get; set; }
    [BindProperty]
    public string Type { get; set; }
    [BindProperty]
    public Guid CourseId { get; set; }
    [BindProperty]
    public Guid OfferingId { get; set; }
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

        AwardType AwardType = AwardType.FromValue(Type);

        if (CurrentStep == Phase.AwardSelection &&
            (AwardType.Equals(AwardType.FirstInSubject) ||
            AwardType.Equals(AwardType.AcademicExcellence) ||
            AwardType.Equals(AwardType.AcademicExcellenceMathematics) ||
            AwardType.Equals(AwardType.AcademicExcellenceScienceTechnology) ||
            AwardType.Equals(AwardType.AcademicAchievement) ||
            AwardType.Equals(AwardType.AcademicAchievementMathematics) ||
            AwardType.Equals(AwardType.AcademicAchievementScienceTechnology)))
        {
            PreviousSteps.Add(Phase.AwardSelection);
            CurrentStep = Phase.CourseSelection;

            await PreparePage();
            return Page();
        }

        if (CurrentStep == Phase.AwardSelection &&
            (AwardType.Equals(AwardType.GalaxyMedal) ||
            AwardType.Equals(AwardType.PrincipalsAward) ||
            AwardType.Equals(AwardType.UniversalAchiever)))
        {
            PreviousSteps.Add(Phase.AwardSelection);
            CurrentStep = Phase.StudentSelection;

            await PreparePage();
            return Page();
        }

        if (CurrentStep == Phase.CourseSelection &&
            (AwardType.Equals(AwardType.AcademicExcellence) ||
            AwardType.Equals(AwardType.AcademicExcellenceMathematics) ||
            AwardType.Equals(AwardType.AcademicExcellenceScienceTechnology) ||
            AwardType.Equals(AwardType.AcademicAchievement) ||
            AwardType.Equals(AwardType.AcademicAchievementMathematics) ||
            AwardType.Equals(AwardType.AcademicAchievementScienceTechnology)))
        {
            PreviousSteps.Add(Phase.CourseSelection);
            CurrentStep = Phase.OfferingSelection;

            await PreparePage();
            return Page();
        }

        if (CurrentStep == Phase.CourseSelection && 
            AwardType.Equals(AwardType.FirstInSubject))
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

        OfferingId offeringId = Core.Models.Offerings.Identifiers.OfferingId.FromValue(OfferingId);
        CourseId courseId = Core.Models.Subjects.Identifiers.CourseId.FromValue(CourseId);

        CreateAwardNominationCommand command = new(
            AwardNominationPeriodId.FromValue(PeriodId),
            AwardType.FromValue(Type),
            courseId,
            offeringId,
            StudentId);

        Result response = await _mediator.Send(command);

        if (response.IsFailure)
        {
            Error = new()
            {
                Error = response.Error,
                RedirectPath = null
            };

            await PreparePage();
            return Page();
        }

        return RedirectToPage("/Awards/Nominations/Details", new { area = "SchoolAdmin", PeriodId = PeriodId });
    }

    private async Task PreparePage()
    {
        AwardNominationPeriodId AwardPeriodId = AwardNominationPeriodId.FromValue(PeriodId);

        Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(AwardPeriodId));

        if (periodRequest.IsFailure)
        {
            Error = new()
            {
                Error = periodRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId })
            };

            return;
        }

        if (CurrentStep == Phase.AwardSelection)
            return;

        if (CurrentStep == Phase.CourseSelection)
        {
            Result<List<CourseSelectListItemResponse>> coursesRequest = await _mediator.Send(new GetCoursesForSelectionListQuery());

            if (coursesRequest.IsFailure)
            {
                Error = new()
                {
                    Error = coursesRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId })
                };

                return;
            }

            Courses = new SelectList(coursesRequest.Value.Where(course => periodRequest.Value.IncludedGrades.Contains(course.Grade)), "Id", "DisplayName", null, "FacultyName");
        }

        if (PreviousSteps.Contains(Phase.CourseSelection))
        {
            Result<List<CourseSelectListItemResponse>> coursesRequest = await _mediator.Send(new GetCoursesForSelectionListQuery());

            if (coursesRequest.IsFailure)
            {
                Error = new()
                {
                    Error = coursesRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId })
                };

                return;
            }

            Courses = new SelectList(coursesRequest.Value.Where(course => periodRequest.Value.IncludedGrades.Contains(course.Grade)), "Id", "DisplayName", CourseId, "FacultyName");
        }

        if (CurrentStep == Phase.OfferingSelection)
        {
            CourseId courseId = Core.Models.Subjects.Identifiers.CourseId.FromValue(CourseId);

            Result<List<OfferingForSelectionList>> offeringRequest = await _mediator.Send(new GetFilteredOfferingsForSelectionListQuery(new List<CourseId> { courseId }));

            if (offeringRequest.IsFailure)
            {
                Error = new()
                {
                    Error = offeringRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId })
                };

                return;
            }

            Offerings = new SelectList(offeringRequest.Value, "Id", "Name");
        }

        if (PreviousSteps.Contains(Phase.OfferingSelection))
        {
            CourseId courseId = Core.Models.Subjects.Identifiers.CourseId.FromValue(CourseId);

            Result<List<OfferingForSelectionList>> offeringRequest = await _mediator.Send(new GetFilteredOfferingsForSelectionListQuery(new List<CourseId> { courseId }));

            if (offeringRequest.IsFailure)
            {
                Error = new()
                {
                    Error = offeringRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId })
                };

                return;
            }

            Offerings = new SelectList(offeringRequest.Value, "Id", "Name", OfferingId);
        }

        if (CurrentStep == Phase.StudentSelection && OfferingId != new Guid())
        {
            OfferingId offeringId = Core.Models.Offerings.Identifiers.OfferingId.FromValue(OfferingId);

            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(new List<Grade>(), new List<OfferingId> { offeringId }, new List<CourseId>()));

            if (studentsRequest.IsFailure)
            {
                Error = new()
                {
                    Error = studentsRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId })
                };

                return;
            }

            StudentsList = studentsRequest.Value;
        }
        else if (CurrentStep == Phase.StudentSelection && CourseId != Guid.Empty)
        {
            CourseId courseId = Core.Models.Subjects.Identifiers.CourseId.FromValue(CourseId);

            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(new List<Grade>(), new List<OfferingId>(), new List<CourseId> { courseId } ));

            if (studentsRequest.IsFailure)
            {
                Error = new()
                {
                    Error = studentsRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId })
                };

                return;
            }

            StudentsList = studentsRequest.Value;
        }
        else if (CurrentStep == Phase.StudentSelection)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(periodRequest.Value.IncludedGrades, new List<OfferingId>(), new List<CourseId>()));

            if (studentsRequest.IsFailure)
            {
                Error = new()
                {
                    Error = studentsRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/SchoolAdmin/Awards/Nominations/Details", values: new { area = "Staff", PeriodId = PeriodId })
                };

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
                CourseId == Guid.Empty)
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
                OfferingId == new Guid())
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
