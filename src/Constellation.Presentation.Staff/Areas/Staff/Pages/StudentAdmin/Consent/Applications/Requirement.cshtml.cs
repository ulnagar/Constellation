namespace Constellation.Presentation.Staff.Areas.Staff.Pages.StudentAdmin.Consent.Applications;

using Application.Courses.GetActiveCoursesList;
using Application.Courses.Models;
using Application.Models.Auth;
using Application.ThirdPartyConsent.CreateRequirement;
using Application.ThirdPartyConsent.GetApplicationById;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Students.GetCurrentStudentsAsDictionary;
using Constellation.Core.Shared;
using Core.Abstractions.Services;
using Core.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Subjects.Identifiers;
using Core.Models.ThirdPartyConsent.Identifiers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Models;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.Threading;

[Authorize(Policy = AuthPolicies.CanEditStudents)]
public class RequirementModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public RequirementModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal)
            .ForContext<RequirementModel>();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.StudentAdmin_Consent_Applications;
    [ViewData] public string PageTitle => "New Application Requirement";


    [BindProperty(SupportsGet=true)]
    public ApplicationId Id { get; set; } = ApplicationId.Empty;
    
    [BindProperty] 
    public List<CourseId> Courses { get; set; } = new();

    [BindProperty] 
    public List<Grade> Grades { get; set; } = new();

    [BindProperty]
    public List<StudentId> Students { get; set; } = new();
    
    
    public ApplicationResponse Application { get; set; }
    public List<CourseSelectListItemResponse> CoursesList { get; set; } = new();
    public Dictionary<StudentId, string> StudentsList { get; set; } = new();


    public async Task OnGet(CancellationToken cancellationToken = default) => await PreparePage(cancellationToken);

    public async Task<IActionResult> OnPost()
    {
        if (Courses.Count == 0 && Grades.Count == 0 && Students.Count == 0)
        {
            ModelState.AddModelError("", "You must provide at least one selection to create a new Requirement");

            await PreparePage();

            return Page();
        }

        foreach (CourseId courseId in Courses)
        {
            Result result = await _mediator.Send(new CreateCourseRequirementCommand(Id, courseId));

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to create new Application Requirement by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    result.Error,
                    _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Details", values: new { area = "Staff", Id }));

                return Page();
            }
        }

        foreach (Grade grade in Grades)
        {
            Result result = await _mediator.Send(new CreateGradeRequirementCommand(Id, grade));

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to create new Application Requirement by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    result.Error,
                    _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Details", values: new { area = "Staff", Id }));

                return Page();
            }
        }

        foreach (StudentId studentId in Students)
        {
            Result result = await _mediator.Send(new CreateStudentRequirementCommand(Id, studentId));

            if (result.IsFailure)
            {
                _logger
                    .ForContext(nameof(Error), result.Error, true)
                    .Warning("Failed to create new Application Requirement by user {User}", _currentUserService.UserName);

                ModalContent = new ErrorDisplay(
                    result.Error,
                    _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Details", values: new { area = "Staff", Id }));

                return Page();
            }
        }

        return RedirectToPage("/StudentAdmin/Consent/Applications/Details", new { area = "Staff", Id});
    }

    private async Task PreparePage(CancellationToken cancellationToken = default)
    {
        Result<ApplicationResponse> applicationResponse = await _mediator.Send(new GetApplicationByIdQuery(Id), cancellationToken);

        if (applicationResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), applicationResponse.Error, true)
                .Warning("Failed to retrieve defaults for new Application Requirement by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                applicationResponse.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Details", values: new { area = "Staff", Id }));

            return;
        }

        Application = applicationResponse.Value;

        Result<List<CourseSelectListItemResponse>> coursesResponse = await _mediator.Send(new GetActiveCoursesListQuery(), cancellationToken);

        if (coursesResponse.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), coursesResponse.Error, true)
                .Warning("Failed to retrieve defaults for new Application Requirement by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                coursesResponse.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Details", values: new { area = "Staff", Id }));

            return;
        }

        CoursesList = coursesResponse.Value;
        
        Result<Dictionary<StudentId, string>> studentsRequest = await _mediator.Send(new GetCurrentStudentsAsDictionaryQuery(), cancellationToken);

        if (studentsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentsRequest.Error, true)
                .Warning("Failed to retrieve defaults for new Application Requirement by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                studentsRequest.Error,
                _linkGenerator.GetPathByPage("/StudentAdmin/Consent/Applications/Details", values: new { area = "Staff", Id }));

            return;
        }

        StudentsList = studentsRequest.Value;
    }
}