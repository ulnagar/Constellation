namespace Constellation.Presentation.Server.Areas.Subject.Pages.Assignments;

using Application.Assignments.CreateAssignment;
using Application.Courses.GetCourseSummary;
using Application.Courses.Models;
using Application.Extensions;
using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Assignments.GetAssignmentsFromCourse;
using Constellation.Application.Courses.GetCoursesForSelectionList;
using Core.Models.Subjects.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class CreateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public CreateModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    public Phase ProgressPhase { get; set; }

    public SelectList Courses { get; set; }
    [BindProperty]
    public Guid Id { get; set; }
    public string CourseName { get; set; }


    public List<SelectListItem> Assignments { get; set; } = new();
    [BindProperty]
    public int CanvasAssignmentId { get; set; }
    public string AssignmentName { get; set; }

    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime DueDate { get; set; }
    [BindProperty]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? UnlockDate { get; set; }
    [BindProperty]
    [DataType(DataType.DateTime)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? LockDate { get; set; }
    [BindProperty]
    public int AllowedAttempts { get; set; }
    [BindProperty]
    public bool DelayForwarding { get; set; }

    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly ForwardingDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task OnGet()
    {
        await GetClasses(_mediator);

        await SetupCourseList();

        ProgressPhase = Phase.SelectCourse;
    }

    private async Task SetupCourseList()
    {
        Result<List<CourseSelectListItemResponse>> courseRequest =
            await _mediator.Send(new GetCoursesForSelectionListQuery());

        if (courseRequest.IsFailure)
        {
            Error = new()
            {
                Error = courseRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Index", values: new { area = "Subject" })
            };

            return;
        }

        Courses = new SelectList(
            courseRequest.Value,
            nameof(CourseSelectListItemResponse.Id),
            nameof(CourseSelectListItemResponse.DisplayName),
            null,
            nameof(CourseSelectListItemResponse.FacultyName));
    }

    public async Task OnPostStepOne()
    {
        await SetupCourseList();

        if (Id == Guid.Empty)
        {
            ModelState.AddModelError(nameof(CourseId), "You must select a valid course");

            await SetupCourseList();

            ProgressPhase = Phase.SelectCourse;

            return;
        }

        await SetupAssignmentList();

        ProgressPhase = Phase.SelectAssignment;
    }

    private async Task SetupAssignmentList()
    {
        CourseId courseId = CourseId.FromValue(Id);

        Result<CourseSummaryResponse> courseRequest = await _mediator.Send(new GetCourseSummaryQuery(courseId));

        if (courseRequest.IsFailure)
        {
            Error = new()
            {
                Error = courseRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Index", values: new { area = "Subject" })
            };

            return;
        }

        CourseName = $"{courseRequest.Value.Grade.AsName()} {courseRequest.Value.Name}";

        Result<List<AssignmentFromCourseResponse>> canvasAssignmentsRequest = await _mediator.Send(new GetAssignmentsFromCourseQuery(courseId));

        if (canvasAssignmentsRequest.IsFailure)
        {
            Error = new()
            {
                Error = canvasAssignmentsRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Index", values: new { area = "Subject" })
            };

            return;
        }

        Assignments = canvasAssignmentsRequest.Value
            .Select(a =>
                new SelectListItem
                {
                    Text = a.Name,
                    Value = a.CanvasAssignmentId.ToString(),
                    Disabled = a.ExistsInDatabase
                })
            .ToList();
    }

    public async Task OnPostStepTwo()
    {
        CourseId courseId = CourseId.FromValue(Id);

        if (CanvasAssignmentId == 0)
        {
            ModelState.AddModelError(nameof(CanvasAssignmentId), "You must select a valid Canvas Assignment");

            await SetupCourseList();

            await SetupAssignmentList();

            ProgressPhase = Phase.SelectAssignment;
            return;
        }

        Result<List<AssignmentFromCourseResponse>> canvasAssignmentsRequest = await _mediator.Send(new GetAssignmentsFromCourseQuery(courseId));

        if (canvasAssignmentsRequest.IsFailure)
        {
            Error = new()
            {
                Error = canvasAssignmentsRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Index", values: new { area = "Subject" })
            };

            return;
        }

        AssignmentFromCourseResponse assignment = canvasAssignmentsRequest.Value.FirstOrDefault(assignment => assignment.CanvasAssignmentId == CanvasAssignmentId);

        if (assignment is null)
        {
            Error = new()
            {
                Error = new("Assignments.Assignment.NotFoundInCanvas", "Could not find the selected Assignment in Canvas"),
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Index", values: new { area = "Subject" })
            };

            return;
        }

        AssignmentName = assignment.Name;

        DueDate = assignment.DueDate;
        UnlockDate = assignment.UnlockDate;
        LockDate = assignment.LockDate;
        AllowedAttempts = assignment.AllowedAttempts;

        ProgressPhase = Phase.EnterDetails;
    }
    
    public async Task<IActionResult> OnPostSubmit()
    {
        CourseId courseId = CourseId.FromValue(Id);

        CreateAssignmentCommand command = new(
            courseId,
            Name,
            CanvasAssignmentId,
            DueDate,
            LockDate,
            UnlockDate,
            AllowedAttempts,
            DelayForwarding,
            ForwardingDate);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            Error = new()
            {
                Error = result.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Assignments/Index", values: new { area = "Subject" })
            };

            return Page();
        }

        return RedirectToPage("/Assignments/Index", new { area = "Subject" });
    }

    public enum Phase
    {
        SelectCourse,
        SelectAssignment,
        EnterDetails
    }

}