namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.GroupTutorials.Tutorials;

using Application.GroupTutorials.AddMultipleStudentsToTutorial;
using Application.GroupTutorials.GetCurrentStudentsInGroupTutorial;
using Application.GroupTutorials.GetTutorialById;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Enrolments.EnrolMultipleStudentsInOffering;
using Constellation.Application.Enrolments.GetCurrentEnrolmentsForOffering;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetOfferingSummary;
using Constellation.Application.Offerings.Models;
using Constellation.Application.Students.GetCurrentStudentsFromGrade;
using Constellation.Application.Students.GetStudentsFromOfferingGrade;
using Constellation.Application.Students.Models;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Staff.Areas;
using Constellation.Presentation.Staff.Areas.Staff.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Helpers.Logging;
using Serilog;

[Authorize(Policy = AuthPolicies.CanEditGroupTutorials)]
public class AddStudentsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public AddStudentsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<AddStudentsModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_GroupTutorials_Tutorials;
    [ViewData] public string PageTitle => "Bulk Add Students";

    [BindProperty(SupportsGet = true)]
    public GroupTutorialId Id { get; set; } = GroupTutorialId.Empty;

    [BindProperty(SupportsGet = true)] 
    public Grade Grade { get; set; } = Grade.Y07;
    
    [BindProperty]
    public List<StudentId> SelectedStudentIds { get; set; } = new();
    public List<StudentFromGradeResponse> Students { get; set; } = new();
    public List<StudentResponse> ExistingEnrolments { get; set; } = new();

    public string TutorialName { get; set; } = string.Empty;

    public async Task OnGet() => await PreparePage();

    public async Task<IActionResult> OnPost()
    {
        AddMultipleStudentsToTutorialCommand command = new(Id, SelectedStudentIds);

        _logger
            .ForContext(nameof(AddMultipleStudentsToTutorialCommand), command, true)
            .Information("Requested to add bulk Students to Group Tutorial by user {User}", _currentUserService.UserName);

        Result request = await _mediator.Send(command);

        if (request.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), request.Error, true)
                .Warning("Failed to add bulk Students to Group Tutorial by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                request.Error,
                _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Details", values: new { area = "Staff", Id = Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Details", new { area = "Staff", Id = Id });
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve defaults to add bulk Students to Group Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<GroupTutorialResponse> tutorial = await _mediator.Send(new GetTutorialByIdQuery(Id));

        if (tutorial.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), tutorial.Error, true)
                .Warning("Failed to retrieve defaults to add bulk Students to Group Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                tutorial.Error,
                _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        TutorialName = tutorial.Value.Name;

        Result<List<StudentResponse>> enrolmentRequest = await _mediator.Send(new GetCurrentStudentsInGroupTutorialQuery(Id));

        if (enrolmentRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), enrolmentRequest.Error, true)
                .Warning("Failed to retrieve defaults to add bulk Students to Group Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                enrolmentRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        ExistingEnrolments = enrolmentRequest.Value;

        Result<List<StudentResponse>> studentsRequest = await _mediator.Send(new GetCurrentStudentsFromGradeQuery(Grade));

        if (studentsRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), studentsRequest.Error, true)
                .Warning("Failed to retrieve defaults to add bulk Students to Group Tutorial with id {Id} by user {User}", Id, _currentUserService.UserName);

            ModalContent = new ErrorDisplay(
                studentsRequest.Error,
                _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Details", values: new { area = "Staff", Id = Id }));

            return;
        }

        Students = studentsRequest.Value
            .Select(entry => 
                new StudentFromGradeResponse(entry.StudentId, entry.Name))
            .ToList();
    }
}