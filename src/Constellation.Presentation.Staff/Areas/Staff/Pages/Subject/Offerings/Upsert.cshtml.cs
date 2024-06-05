namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Offerings;

using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.CreateOffering;
using Constellation.Application.Offerings.GetOfferingSummary;
using Constellation.Application.Offerings.Models;
using Constellation.Application.Offerings.UpdateOffering;
using Constellation.Core.Abstractions.Clock;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly IDateTimeProvider _dateTime;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        IDateTimeProvider dateTime)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _dateTime = dateTime;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Subject_Offerings_Offerings;

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    public Guid CourseId { get; set; }
    public string CourseName { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly StartDate { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
    public DateOnly EndDate { get; set; }
    public SelectList CourseList { get; set; }

    public async Task OnGet()
    {
        if (Id.HasValue)
        {
            OfferingId offeringId = OfferingId.FromValue(Id.Value);

            Result<OfferingSummaryResponse> offering = await _mediator.Send(new GetOfferingSummaryQuery(offeringId));

            if (offering.IsFailure)
            {
                Error = new()
                {
                    Error = offering.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" })
                };

                return;
            }

            Name = offering.Value.Name;
            CourseName = offering.Value.CourseName;
            StartDate = offering.Value.StartDate;
            EndDate = offering.Value.EndDate;
        }
        else
        {
            StartDate = _dateTime.Today;
            EndDate = _dateTime.LastDayOfYear;

            await BuildCourseSelectList();
        }

        return;
    }

    public async Task<IActionResult> OnPostCreate()
    {
        if (!ModelState.IsValid)
        {
            await BuildCourseSelectList();

            return Page();
        }

        CourseId courseId = Core.Models.Subjects.Identifiers.CourseId.FromValue(CourseId);

        Result<OfferingId> request = await _mediator.Send(new CreateOfferingCommand(Name, courseId, StartDate, EndDate));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" })
            };

            return Page();
        }

        return RedirectToPage("/Subject/Offerings/Details", new { area = "Staff", Id = request.Value.Value });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        OfferingId offeringId = OfferingId.FromValue(Id.Value);

        Result request = await _mediator.Send(new UpdateOfferingCommand(offeringId, StartDate, EndDate));

        if (request.IsFailure)
        {
            Result<OfferingSummaryResponse> offering = await _mediator.Send(new GetOfferingSummaryQuery(offeringId));

            if (offering.IsFailure)
            {
                Error = new()
                {
                    Error = offering.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Index", values: new { area = "Staff" })
                };

                return Page();
            }

            Name = offering.Value.Name;
            CourseName = offering.Value.CourseName;

            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Offerings/Details", values: new { area = "Staff", Id = Id.Value })
            };

            return Page();
        }

        return RedirectToPage("/Subject/Offerings/Details", new { area = "Staff", Id = Id.Value });
    }

    private async Task BuildCourseSelectList()
    {
        Result<List<CourseSelectListItemResponse>> coursesResponse = await _mediator.Send(new GetCoursesForSelectionListQuery());

        if (coursesResponse.IsFailure)
        {
            Error = new()
            {
                Error = coursesResponse.Error,
                RedirectPath = null
            };

            return;
        }

        CourseList = new SelectList(coursesResponse.Value, "Id", "DisplayName", null, "FacultyName");
    }
}
