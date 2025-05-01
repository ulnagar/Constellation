namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Assignments;

using Application.Common.PresentationModels;
using Application.Domains.Assignments.Commands.CreateAssignment;
using Application.Domains.Assignments.Models;
using Application.Domains.Assignments.Queries.GetUploadAssignmentsFromCourse;
using Application.Domains.Courses.Queries.GetCoursesForSelectionList;
using Application.Domains.Courses.Queries.GetCourseSummary;
using Constellation.Application.Domains.Courses.Models;
using Constellation.Application.Models.Auth;
using Constellation.Core.Extensions;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.ComponentModel.DataAnnotations;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class CreateModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public CreateModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<CreateModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Assignments_Assignments;
    [ViewData] public string PageTitle => "New Assignment";

    public Phase ProgressPhase { get; set; }

    public SelectList Courses { get; set; }
    [BindProperty]
    public CourseId Id { get; set; } = CourseId.Empty;
    public string CourseName { get; set; }


    public List<SelectListItem> Assignments { get; set; } = new();
    [BindProperty]
    public int CanvasAssignmentId { get; set; }
    public string AssignmentName { get; set; }

    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    [DataType(DataType.DateTime)]
    public DateTime DueDate { get; set; }
    [BindProperty]
    [DataType(DataType.DateTime)]
    public DateTime? UnlockDate { get; set; }
    [BindProperty]
    [DataType(DataType.DateTime)]
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
        _logger.Information("Requested to retrieve course list for new Assignment by user {User}", _currentUserService.UserName);

        Result<List<CourseSelectListItemResponse>> courseRequest = await _mediator.Send(new GetCoursesForSelectionListQuery());

        if (courseRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), courseRequest.Error, true)
                .Warning("Failed to retrieve course list for new Assignment by user {User}", _currentUserService.UserName);
            
            ModalContent = new ErrorDisplay(
                courseRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

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

        if (Id == CourseId.Empty)
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
        _logger.Information("Requested to retrieve course list for new Assignment by user {User}", _currentUserService.UserName);

        Result<CourseSummaryResponse> courseRequest = await _mediator.Send(new GetCourseSummaryQuery(Id));

        if (courseRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), courseRequest.Error, true)
                .Warning("Failed to retrieve course list for new Assignment by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                courseRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

            return;
        }

        CourseName = $"{courseRequest.Value.Grade.AsName()} {courseRequest.Value.Name}";

        _logger.Information("Requested to retrieve Assignment list from Canvas for new Assignment by user {User}", _currentUserService.UserName);

        Result<List<AssignmentFromCourseResponse>> canvasAssignmentsRequest = await _mediator.Send(new GetUploadAssignmentsFromCourseQuery(Id));

        if (canvasAssignmentsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), canvasAssignmentsRequest.Error, true)
                .Warning("Failed to retrieve Assignment list from Canvas for new Assignment by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                canvasAssignmentsRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

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
        if (CanvasAssignmentId == 0)
        {
            ModelState.AddModelError(nameof(CanvasAssignmentId), "You must select a valid Canvas Assignment");

            await SetupCourseList();

            await SetupAssignmentList();

            ProgressPhase = Phase.SelectAssignment;
            return;
        }

        _logger.Information("Requested to retrieve course list for new Assignment by user {User}", _currentUserService.UserName);

        Result<CourseSummaryResponse> courseRequest = await _mediator.Send(new GetCourseSummaryQuery(Id));

        if (courseRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), courseRequest.Error, true)
                .Warning("Failed to retrieve course list for new Assignment by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                courseRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

            return;
        }

        CourseName = $"{courseRequest.Value.Grade.AsName()} {courseRequest.Value.Name}";

        _logger.Information("Requested to retrieve Assignment list from Canvas for new Assignment by user {User}", _currentUserService.UserName);
        
        Result<List<AssignmentFromCourseResponse>> canvasAssignmentsRequest = await _mediator.Send(new GetUploadAssignmentsFromCourseQuery(Id));

        if (canvasAssignmentsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), canvasAssignmentsRequest.Error, true)
                .Warning("Failed to retrieve Assignment list from Canvas for new Assignment by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                canvasAssignmentsRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

            return;
        }

        AssignmentFromCourseResponse? assignment = canvasAssignmentsRequest.Value.FirstOrDefault(assignment => assignment.CanvasAssignmentId == CanvasAssignmentId);

        if (assignment is null)
        {
            ModalContent = new ErrorDisplay(
                new("Assignments.Assignment.NotFoundInCanvas", "Could not find the selected Assignment in Canvas"),
                _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));

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
        CreateAssignmentCommand command = new(
            Id,
            Name,
            CanvasAssignmentId,
            DueDate,
            LockDate,
            UnlockDate,
            AllowedAttempts,
            DelayForwarding,
            ForwardingDate);

        _logger
            .ForContext(nameof(CreateAssignmentCommand), command, true)
            .Information("Requested to create new Assignment by user {User}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to create new Assignment by user {User}", _currentUserService.UserName);

            if (result is IValidationResult validationResult)
            {
                ModalContent = new ErrorDisplay(
                    validationResult.Errors.First(),
                    _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));
            }
            else
            {
                ModalContent = new ErrorDisplay(
                    result.Error,
                    _linkGenerator.GetPathByPage("/Subject/Assignments/Index", values: new { area = "Staff" }));
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