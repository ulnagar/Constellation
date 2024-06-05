namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assignments;

using Constellation.Application.Assignments.CreateAssignment;
using Constellation.Application.Assignments.GetAssignmentsFromCourse;
using Constellation.Application.Courses.GetCoursesForSelectionList;
using Constellation.Application.Courses.GetCourseSummary;
using Constellation.Application.Courses.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Extensions;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
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

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assignments_Assignments;

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
    //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime DueDate { get; set; }
    [BindProperty]
    [DataType(DataType.DateTime)]
    //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? UnlockDate { get; set; }
    [BindProperty]
    [DataType(DataType.DateTime)]
    //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? LockDate { get; set; }
    [BindProperty]
    public int AllowedAttempts { get; set; }
    [BindProperty]
    public bool DelayForwarding { get; set; } = true;

    [BindProperty]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly ForwardingDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task OnGet()
    {
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
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" })
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
        
        Result<CourseSummaryResponse> courseRequest = await _mediator.Send(new GetCourseSummaryQuery(courseId));

        if (courseRequest.IsFailure)
        {
            Error = new()
            {
                Error = courseRequest.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" })
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
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" })
            };

            return;
        }

        AssignmentFromCourseResponse? assignment = canvasAssignmentsRequest.Value.FirstOrDefault(assignment => assignment.CanvasAssignmentId == CanvasAssignmentId);

        if (assignment is null)
        {
            Error = new()
            {
                Error = new("Assignments.Assignment.NotFoundInCanvas", "Could not find the selected Assignment in Canvas"),
                RedirectPath = _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" })
            };

            return;
        }

        AssignmentName = assignment.Name;

        DueDate = assignment.DueDate;
        UnlockDate = assignment.UnlockDate;
        LockDate = assignment.LockDate;
        AllowedAttempts = assignment.AllowedAttempts;

        if (LockDate.HasValue)
            ForwardingDate = DateOnly.FromDateTime(LockDate.Value);

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
            if (result is IValidationResult validationResult)
            {
                Error = new()
                {
                    Error = validationResult.Errors.First(),
                    RedirectPath = _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" })
                };
            }
            else
            {
                Error = new()
                {
                    Error = result.Error,
                    RedirectPath = _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" })
                };
            }

            return Page();
        }

        return RedirectToPage("/Subject/Assignments/Index", new { area = "Staff" });
    }

    public enum Phase
    {
        SelectCourse,
        SelectAssignment,
        EnterDetails
    }

}