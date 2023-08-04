namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Awards.Nominations;

using Constellation.Application.Awards.CreateAwardNomination;
using Constellation.Application.Awards.GetAllNominationPeriods;
using Constellation.Application.Awards.GetNominationPeriod;
using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetFilteredOfferingsForSelectionList;
using Constellation.Application.Students.GetFilteredStudentsForSelectionList;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Core.ValueObjects;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    [ViewData]
    public string ActivePage => "Nominations";

    [BindProperty(SupportsGet = true)]
    public Guid PeriodId { get; set; }

    public NominationPeriodResponse Period { get; set; }

    [BindProperty]
    public string StudentId { get; set; }
    [BindProperty]
    public string Type { get; set; }
    [BindProperty]
    public int CourseId { get; set; }
    [BindProperty]
    public int OfferingId { get; set; }
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
            AwardType.Equals(AwardType.AcademicAchievement)))
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
            AwardType.Equals(AwardType.AcademicAchievement)))
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

        CreateAwardNominationCommand command = new(
            AwardNominationPeriodId.FromValue(PeriodId),
            AwardType.FromValue(Type),
            CourseId,
            OfferingId,
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
        await GetClasses(_mediator);

        AwardNominationPeriodId AwardPeriodId = AwardNominationPeriodId.FromValue(PeriodId);

        Result<NominationPeriodDetailResponse> periodRequest = await _mediator.Send(new GetNominationPeriodRequest(AwardPeriodId));

        if (periodRequest.IsFailure)
        {
            Error = new()
            {
                Error = periodRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Details", values: new { area = "SchoolAdmin", PeriodId = PeriodId })
            };

            return;
        }

        if (CurrentStep == Phase.AwardSelection)
            return;

        if (CurrentStep == Phase.CourseSelection)
        {
            Result<List<CourseSummaryResponse>> coursesRequest = await _mediator.Send(new GetCoursesForSelectionListQuery());

            if (coursesRequest.IsFailure)
            {
                Error = new()
                {
                    Error = coursesRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Details", values: new { area = "SchoolAdmin", PeriodId = PeriodId })
                };

                return;
            }

            Courses = new SelectList(coursesRequest.Value.Where(course => periodRequest.Value.IncludedGrades.Contains(course.Grade)), "Id", "DisplayName", null, "FacultyName");
        }

        if (PreviousSteps.Contains(Phase.CourseSelection))
        {
            Result<List<CourseSummaryResponse>> coursesRequest = await _mediator.Send(new GetCoursesForSelectionListQuery());

            if (coursesRequest.IsFailure)
            {
                Error = new()
                {
                    Error = coursesRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Details", values: new { area = "SchoolAdmin", PeriodId = PeriodId })
                };

                return;
            }

            Courses = new SelectList(coursesRequest.Value.Where(course => periodRequest.Value.IncludedGrades.Contains(course.Grade)), "Id", "DisplayName", CourseId, "FacultyName");
        }

        if (CurrentStep == Phase.OfferingSelection)
        {
            Result<List<OfferingForSelectionList>> offeringRequest = await _mediator.Send(new GetFilteredOfferingsForSelectionListQuery(new List<int> { CourseId }));

            if (offeringRequest.IsFailure)
            {
                Error = new()
                {
                    Error = offeringRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Details", values: new { area = "SchoolAdmin", PeriodId = PeriodId })
                };

                return;
            }

            Offerings = new SelectList(offeringRequest.Value, "Id", "Name");
        }

        if (PreviousSteps.Contains(Phase.OfferingSelection))
        {
            Result<List<OfferingForSelectionList>> offeringRequest = await _mediator.Send(new GetFilteredOfferingsForSelectionListQuery(new List<int> { CourseId }));

            if (offeringRequest.IsFailure)
            {
                Error = new()
                {
                    Error = offeringRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Details", values: new { area = "SchoolAdmin", PeriodId = PeriodId })
                };

                return;
            }

            Offerings = new SelectList(offeringRequest.Value, "Id", "Name", OfferingId);
        }

        if (CurrentStep == Phase.StudentSelection && OfferingId > 0)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(periodRequest.Value.IncludedGrades, new List<int> { OfferingId }, new List<int>()));

            if (studentsRequest.IsFailure)
            {
                Error = new()
                {
                    Error = studentsRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Details", values: new { area = "SchoolAdmin", PeriodId = PeriodId })
                };

                return;
            }

            StudentsList = studentsRequest.Value;
        }
        else if (CurrentStep == Phase.StudentSelection && CourseId > 0)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(new List<Grade>(), new List<int>(), new List<int> { CourseId } ));

            if (studentsRequest.IsFailure)
            {
                Error = new()
                {
                    Error = studentsRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Details", values: new { area = "SchoolAdmin", PeriodId = PeriodId })
                };

                return;
            }

            StudentsList = studentsRequest.Value;
        }
        else if (CurrentStep == Phase.StudentSelection)
        {
            Result<List<StudentForSelectionList>> studentsRequest = await _mediator.Send(new GetFilteredStudentsForSelectionListQuery(periodRequest.Value.IncludedGrades, new List<int>(), new List<int>()));

            if (studentsRequest.IsFailure)
            {
                Error = new()
                {
                    Error = studentsRequest.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Details", values: new { area = "SchoolAdmin", PeriodId = PeriodId })
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
                Type != AwardType.AcademicAchievement.Value &&
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
                Type == AwardType.AcademicAchievement.Value) &&
                CourseId == 0)
            {
                ModelState.AddModelError("CourseId", "You must select a valid course");
            }
        }

        if (CurrentStep == Phase.OfferingSelection || PreviousSteps.Contains(Phase.OfferingSelection))
        {
            if ((Type == AwardType.AcademicExcellence.Value ||
                Type == AwardType.AcademicAchievement.Value) &&
                OfferingId == 0)
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
