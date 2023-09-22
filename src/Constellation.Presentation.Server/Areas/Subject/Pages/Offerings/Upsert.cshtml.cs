namespace Constellation.Presentation.Server.Areas.Subject.Pages.Offerings;

using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.CreateOffering;
using Constellation.Application.Offerings.GetOfferingSummary;
using Constellation.Application.Offerings.Models;
using Constellation.Application.Offerings.UpdateOffering;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    public Guid CourseId { get; set; }
    public string CourseName { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    public DateOnly StartDate { get; set; }
    [BindProperty]
    [DataType(DataType.Date)]
    public DateOnly EndDate { get; set; }
    public SelectList CourseList { get; set; }

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            OfferingId offeringId = OfferingId.FromValue(Id.Value);

            Result<OfferingSummaryResponse> offering = await _mediator.Send(new GetOfferingSummaryQuery(offeringId));

            if (offering.IsFailure)
            {
                Error = new()
                {
                    Error = offering.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Index", values: new { area = "Subject" })
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
            await BuildCourseSelectList();
        }

        return;
    }

    public async Task<IActionResult> OnPostCreate()
    {
        if (!ModelState.IsValid)
        {
            await GetClasses(_mediator);
            await BuildCourseSelectList();

            return Page();
        }

        CourseId courseId = Core.Models.Subjects.Identifiers.CourseId.FromValue(CourseId);

        Result<OfferingId> request = await _mediator.Send(new CreateOfferingCommand(Name, courseId, StartDate, EndDate));

        if (request.IsFailure)
        {
            await GetClasses(_mediator);

            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Index", values: new { area = "Subject" })
            };

            return Page();
        }

        return RedirectToPage("/Offerings/Details", new { area = "Subject", Id = request.Value.Value });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if (!ModelState.IsValid)
        {
            await GetClasses(_mediator);

            return Page();
        }

        OfferingId offeringId = OfferingId.FromValue(Id.Value);

        Result request = await _mediator.Send(new UpdateOfferingCommand(offeringId, StartDate, EndDate));

        if (request.IsFailure)
        {
            await GetClasses(_mediator);

            Result<OfferingSummaryResponse> offering = await _mediator.Send(new GetOfferingSummaryQuery(offeringId));

            if (offering.IsFailure)
            {
                Error = new()
                {
                    Error = offering.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Index", values: new { area = "Subject" })
                };

                return Page();
            }

            Name = offering.Value.Name;
            CourseName = offering.Value.CourseName;

            Error = new()
            {
                Error = request.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Offerings/Details", values: new { area = "Subject", Id = Id.Value })
            };

            return Page();
        }

        return RedirectToPage("/Offerings/Details", new { area = "Subject", Id = Id.Value });
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
