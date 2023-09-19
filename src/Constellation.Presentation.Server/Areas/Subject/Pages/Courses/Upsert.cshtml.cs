namespace Constellation.Presentation.Server.Areas.Subject.Pages.Courses;

using Constellation.Application.Courses.CreateCourse;
using Constellation.Application.Courses.GetCourseSummary;
using Constellation.Application.Courses.Models;
using Constellation.Application.Courses.UpdateCourse;
using Constellation.Application.Faculties.GetFacultiesForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;

    public UpsertModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    public string Code { get; set; }
    [BindProperty]
    public Grade Grade { get; set; }
    [BindProperty]
    public Guid FacultyId { get; set; }
    [BindProperty]
    public decimal FTEValue { get; set; }

    public List<FacultySummaryResponse> Faculties { get; set; } = new();

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        if (Id.HasValue)
        {
            CourseId courseId = CourseId.FromValue(Id.Value);

            Result<CourseSummaryResponse> courseRequest = await _mediator.Send(new GetCourseSummaryQuery(courseId));

            if (courseRequest.IsFailure)
            {
                Error = new()
                {
                    Error = courseRequest.Error,
                    RedirectPath = null
                };

                return;
            }

            Name = courseRequest.Value.Name;
            Code = courseRequest.Value.Code;
            Grade = courseRequest.Value.Grade;
            FacultyId = courseRequest.Value.CourseFaculty.FacultyId;
            FTEValue = courseRequest.Value.FTEValue;
        }

        Result<List<FacultySummaryResponse>> facultyRequest = await _mediator.Send(new GetFacultiesForSelectionListQuery());

        if (facultyRequest.IsFailure)
        {
            Error = new()
            {
                Error = facultyRequest.Error,
                RedirectPath = null
            };

            return;
        }

        Faculties = facultyRequest.Value;
    }

    public async Task<IActionResult> OnPostCreate()
    {
        if (!ModelState.IsValid)
        {
            Result<List<FacultySummaryResponse>> facultyRequest = await _mediator.Send(new GetFacultiesForSelectionListQuery());

            if (facultyRequest.IsFailure)
            {
                Error = new()
                {
                    Error = facultyRequest.Error,
                    RedirectPath = null
                };

                return Page();
            }

            Faculties = facultyRequest.Value;

            return Page();
        }

        Result request = await _mediator.Send(new CreateCourseCommand(
            Name,
            Code,
            Grade,
            FacultyId,
            FTEValue));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            Result<List<FacultySummaryResponse>> facultyRequest = await _mediator.Send(new GetFacultiesForSelectionListQuery());

            if (facultyRequest.IsFailure)
            {
                Error = new()
                {
                    Error = facultyRequest.Error,
                    RedirectPath = null
                };

                return Page();
            }

            Faculties = facultyRequest.Value;

            return Page();
        }

        return RedirectToPage("/Courses/Index", new { area = "Subject" });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if(!ModelState.IsValid)
        {
            Result<List<FacultySummaryResponse>> facultyRequest = await _mediator.Send(new GetFacultiesForSelectionListQuery());

            if (facultyRequest.IsFailure)
            {
                Error = new()
                {
                    Error = facultyRequest.Error,
                    RedirectPath = null
                };

                return Page();
            }

            Faculties = facultyRequest.Value;

            return Page();
        }

        CourseId courseId = CourseId.FromValue(Id.Value);

        Result request = await _mediator.Send(new UpdateCourseCommand(
            courseId,
            Name,
            Code,
            Grade,
            FacultyId,
            FTEValue));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            Result<List<FacultySummaryResponse>> facultyRequest = await _mediator.Send(new GetFacultiesForSelectionListQuery());

            if (facultyRequest.IsFailure)
            {
                Error = new()
                {
                    Error = facultyRequest.Error,
                    RedirectPath = null
                };

                return Page();
            }

            Faculties = facultyRequest.Value;

            return Page();
        }

        return RedirectToPage("/Courses/Index", new { area = "Subject" });
    }
}
