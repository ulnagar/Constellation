namespace Constellation.Presentation.Students.Areas.Students.Pages.Tutorials;

using Application.Domains.Courses.Queries.GetCoursesForStudent;
using Application.Domains.Timetables.Timetables.Queries.GetStudentTimetableData;
using Application.Domains.Tutorials.Requests.Commands.CreateTutorialRequest;
using Application.DTOs;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.Students.Errors;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Models.Subjects.Identifiers;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials.Enums;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.ModelBinders;
using Serilog;

[HasPermission(AuthPermission.StudentPortal_View_Value)]
public class RequestModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public RequestModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<RequestModel>()
            .ForStudentPortal();
    }
    
    [ViewData] public string ActivePage => Models.ActivePage.Tutorials;

    [BindProperty(SupportsGet = true)]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public TutorialType Type { get; set; } = TutorialType.Unknown;

    [BindProperty(SupportsGet = true)]
    public CourseId CourseId { get; set; } = CourseId.Empty;

    [BindProperty]
    public List<PeriodId> PeriodIds { get; set; }
    [BindProperty]
    public string Comment { get; set; } = string.Empty;

    public List<StudentCourseResponse> Courses { get; set; } = [];
    public StudentCourseResponse? Course { get; set; }
    public StudentTimetableDataDto Periods { get; set; }

    public async Task OnGet()
    {
        _logger.Information("Requested to retrieve course data by user {user}", _currentUserService.UserName);

        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve course data by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(StudentErrors.InvalidId);

            return;
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));

        Result<List<StudentCourseResponse>> courseRequest = await _mediator.Send(new GetCoursesForStudentQuery(studentId));

        if (courseRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), courseRequest.Error, true)
                .Warning("Failed to retrieve course data by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(courseRequest.Error);

            return;
        }

        Courses = courseRequest.Value;
        if (CourseId != CourseId.Empty)
        {
            Course = Courses
                .FirstOrDefault(entry => entry.Id == CourseId);
        }

        Result<StudentTimetableDataDto> periodRequest = await _mediator.Send(new GetStudentTimetableDataQuery(studentId));

        if (periodRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), periodRequest.Error, true)
                .Warning("Failed to retrieve course data by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(periodRequest.Error);

            return;
        }

        Periods = periodRequest.Value;
    }

    public async Task<IActionResult> OnPost()
    {
        string studentIdClaimValue = User.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StudentId)?.Value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(studentIdClaimValue))
        {
            _logger
                .ForContext(nameof(Error), StudentErrors.InvalidId, true)
                .Warning("Failed to retrieve course data by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                StudentErrors.InvalidId,
                _linkGenerator.GetPathByPage("/Tutorials/Request", values: new { area = "Students" }));

            return Page();
        }

        StudentId studentId = StudentId.FromValue(new(studentIdClaimValue));

        CreateTutorialRequestCommand command = new(
            studentId,
            Type,
            CourseId,
            PeriodIds,
            Comment);

        _logger
            .ForContext(nameof(CreateTutorialRequestCommand), command, true)
            .Information("Requested to enter Tutorial Request by user {user}", _currentUserService.UserName);

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(CreateTutorialRequestCommand), command, true)
                .ForContext(nameof(Error), result.Error, true)
                .Information("Failed to enter Tutorial Request by user {user}", _currentUserService.UserName);

            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Tutorials/Request", values: new { area = "Students" }));

            return Page();
        }

        return RedirectToPage("/Tutorials/Index", new { area = "Students" });
    }
}