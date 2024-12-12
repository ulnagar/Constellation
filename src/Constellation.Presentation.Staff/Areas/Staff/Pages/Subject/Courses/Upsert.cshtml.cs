namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Courses;

using Application.Common.PresentationModels;
using Constellation.Application.Courses.CreateCourse;
using Constellation.Application.Courses.GetCourseSummary;
using Constellation.Application.Courses.Models;
using Constellation.Application.Courses.UpdateCourse;
using Constellation.Application.Faculties.GetFacultiesForSelectionList;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Models.Faculties.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditSubjects)]
public class UpsertModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public UpsertModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<UpsertModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Courses_Courses;
    [ViewData] public string PageTitle { get; set; } = "New Course";

    [BindProperty(SupportsGet = true)]
    public CourseId Id { get; set; } = CourseId.Empty;

    [BindProperty]
    public string Name { get; set; }
    [BindProperty]
    public string Code { get; set; }
    [BindProperty]
    public Grade Grade { get; set; }

    [BindProperty]
    public FacultyId FacultyId { get; set; } = FacultyId.Empty;
    [BindProperty]
    public decimal FTEValue { get; set; }
    [BindProperty]
    public double TargetPerCycle { get; set; }

    public List<FacultySummaryResponse> Faculties { get; set; } = new();

    public async Task OnGet()
    {
        if (Id != CourseId.Empty)
        {
            _logger.Information("Requested to retrieve Course with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

            Result<CourseSummaryResponse> courseRequest = await _mediator.Send(new GetCourseSummaryQuery(Id));

            if (courseRequest.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), courseRequest.Error, true)
                    .Warning("Failed to retrieve Course with id {Id} for edit by user {User}", Id, _currentUserService.UserName);

                ModalContent = new ErrorDisplay(courseRequest.Error);

                return;
            }

            Name = courseRequest.Value.Name;
            Code = courseRequest.Value.Code;
            Grade = courseRequest.Value.Grade;
            FacultyId = courseRequest.Value.CourseFaculty.FacultyId;
            FTEValue = courseRequest.Value.FTEValue;
            TargetPerCycle = courseRequest.Value.TargetPerCycle;

            PageTitle = $"Edit - {Name}";
        }

        await PreparePage();
    }

    public async Task<IActionResult> OnPostCreate()
    {
        if (!ModelState.IsValid)
        {
            await PreparePage();

            return Page();
        }

        CreateCourseCommand command = new(
            Name,
            Code,
            Grade,
            FacultyId,
            FTEValue,
            TargetPerCycle);

        _logger
            .ForContext(nameof(CreateCourseCommand), command, true)
            .Information("Requested to create new Course by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to create new Course by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/Courses/Index", new { area = "Staff" });
    }

    public async Task<IActionResult> OnPostUpdate()
    {
        if(!ModelState.IsValid)
        {
            await PreparePage();

            return Page();
        }

        UpdateCourseCommand command = new(
            Id,
            Name,
            Code,
            Grade,
            FacultyId,
            FTEValue,
            TargetPerCycle);

        _logger
            .ForContext(nameof(UpdateCourseCommand), command, true)
            .Information("Requested to update Course with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to update Course with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(request.Error);

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/Courses/Index", new { area = "Staff" });
    }

    private async Task PreparePage()
    {
        Result<List<FacultySummaryResponse>> facultyRequest = await _mediator.Send(new GetFacultiesForSelectionListQuery());

        if (facultyRequest.IsFailure)
        {
            ModalContent = new ErrorDisplay(facultyRequest.Error);

            return;
        }

        Faculties = facultyRequest.Value;
    }
}
