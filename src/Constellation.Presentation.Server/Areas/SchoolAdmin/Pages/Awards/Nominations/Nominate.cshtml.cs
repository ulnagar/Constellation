namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Awards.Nominations;

using Constellation.Application.Awards.GetAllNominationPeriods;
using Constellation.Application.Awards.GetNominationPeriod;
using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Application.Offerings.GetOfferingsForSelectionList;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.ValueObjects;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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

    public Dictionary<string, string> StudentsList { get; set; }
    public SelectList Courses { get; set; }
    public List<OfferingSelectionListResponse> Offerings { get; set; }

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

        // Persist new entry

        return RedirectToPage("/Awards/Nominations/Details", new { area = "SchoolAdmin", PeriodId = PeriodId });
    }

    private async Task PreparePage()
    {
        await GetClasses(_mediator);

        var AwardPeriodId = AwardNominationPeriodId.FromValue(PeriodId);

        var periodRequest = await _mediator.Send(new GetNominationPeriodRequest(AwardPeriodId));

        // Filter students and courses by the grades that were selected in the NominationPeriod entry
        // Filter offerings based on the NominationPeriod first, then on the selected course later

        var studentsRequest = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery());

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

        var offeringRequest = await _mediator.Send(new GetOfferingsForSelectionListQuery());

        if (offeringRequest.IsFailure)
        {
            Error = new()
            {
                Error = offeringRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Details", values: new { area = "SchoolAdmin", PeriodId = PeriodId })
            };

            return;
        }

        Offerings = offeringRequest.Value;

        var coursesRequest = await _mediator.Send(new GetCoursesForSelectionListQuery());

        if (coursesRequest.IsFailure)
        {
            Error = new()
            {
                Error = coursesRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Awards/Nominations/Details", values: new { area = "SchoolAdmin", PeriodId = PeriodId })
            };

            return;
        }

        Courses = new SelectList(coursesRequest.Value, "Id", "DisplayName", null, "FacultyName");
    }

    private void ValidateEntries()
    {
        if (StudentId is null)
        {
            ModelState.AddModelError("StudentId", "You must select a student");
        }

        if (Type is null)
        {
            ModelState.AddModelError("AwardType", "You must select an award type");
        }

        if (Type != AwardType.FirstInSubject.Value ||
            Type != AwardType.AcademicExcellence.Value ||
            Type != AwardType.AcademicAchievement.Value ||
            Type != AwardType.PrincipalsAward.Value ||
            Type != AwardType.GalaxyMedal.Value ||
            Type != AwardType.UniversalAchiever.Value)
        {
            ModelState.AddModelError("AwardType", "You must select a valid award type");
        }

        if ((Type == AwardType.FirstInSubject.Value ||
            Type == AwardType.AcademicExcellence.Value ||
            Type == AwardType.AcademicAchievement.Value) &&
            CourseId == 0)
        {
            ModelState.AddModelError("CourseId", "You must select a valid course");
        }

        if ((Type == AwardType.AcademicExcellence.Value ||
            Type == AwardType.AcademicAchievement.Value) &&
            OfferingId == 0)
        {
            ModelState.AddModelError("OfferingId", "You must select a valid class");
        }
    }
}
