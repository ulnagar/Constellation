namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.Tutorials.GroupTutorials;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.GroupTutorials.Commands.AddStudentToTutorialRoll;
using Constellation.Application.Domains.GroupTutorials.Commands.RemoveStudentFromTutorialRoll;
using Constellation.Application.Domains.GroupTutorials.Commands.SubmitRoll;
using Constellation.Application.Domains.GroupTutorials.Queries.GetTutorialRollWithDetailsById;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.Students.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Shared.Helpers.Logging;
using Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TutorialRollAddStudent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Serilog;

[Authorize(Policy = AuthPolicies.CanViewGroupTutorials)]
public class RollModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger _logger;

    public RollModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
        _logger = logger
            .ForContext<RollModel>()
            .ForContext(LogDefaults.Application, LogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_Tutorials_GroupTutorials;
    [ViewData] public string PageTitle { get; set; } = "Group Tutorial Roll";


    [BindProperty(SupportsGet = true)]
    public GroupTutorialId TutorialId { get; set; }

    [BindProperty(SupportsGet = true)]
    public TutorialRollId RollId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Mode { get; set; }

    [BindProperty] 
    public List<TutorialRollStudentResponse> Students { get; set; } = new();

    public TutorialRollDetailResponse Roll { get; set; }

    [BindProperty]
    public TutorialRollAddStudentSelection AddStudentSelection { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (Mode == "Edit")
        {
            AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, TutorialId, AuthPolicies.CanSubmitGroupTutorialRolls);

            if (!isAuthorised.Succeeded)
            {
                return ShowError(DomainErrors.Permissions.Unauthorised);
            }
        }

        await PreparePage();

        return Page();
    }

    public async Task<IActionResult> OnPostSubmit()
    {
        string emailAddress = _currentUserService.EmailAddress;

        Dictionary<StudentId, bool> studentData = Students.ToDictionary(k => k.StudentId, k => k.Present);

        SubmitRollCommand command = new(
            TutorialId,
            RollId,
            emailAddress,
            studentData);

        _logger
            .ForContext(nameof(SubmitRollCommand), command, true)
            .Information("Requested to submit Group Tutorial Roll by user {User}", _currentUserService.UserName);
        
        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, TutorialId, AuthPolicies.CanSubmitGroupTutorialRolls);

        if (!isAuthorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Permissions.Unauthorised, true)
                .Warning("Failed to submit Group Tutorial Roll by user {User}", _currentUserService.UserName);

            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to submit Group Tutorial Roll by user {User}", _currentUserService.UserName);

            return ShowError(result.Error);
        }

        return RedirectToPage("/Subject/Tutorials/GroupTutorials/Details", new { area = "Staff", Id = TutorialId });
    }

    public async Task<IActionResult> OnPostAddStudent()
    {
        AddStudentToTutorialRollCommand command = new(TutorialId, RollId, AddStudentSelection.StudentId);

        _logger
            .ForContext(nameof(AddStudentToTutorialRollCommand), command, true)
            .Information("Requested to add student to Group Tutorial Roll by user {User}", _currentUserService.UserName);

        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, TutorialId, AuthPolicies.CanSubmitGroupTutorialRolls);

        if (!isAuthorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Auth.NotAuthorised, true)
                .Warning("Failed to add student to Group Tutorial Roll by user {User}", _currentUserService.UserName);

            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        if (AddStudentSelection.StudentId == StudentId.Empty)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.GroupTutorials.TutorialRoll.StudentNotFound(AddStudentSelection.StudentId), true)
                .Warning("Failed to add student to Group Tutorial Roll by user {User}", _currentUserService.UserName);

            await PreparePage();
            return Page();
        }

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to add student to Group Tutorial Roll by user {User}", _currentUserService.UserName);

            return ShowError(result.Error);
        }

        AddStudentSelection = new();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetRemoveStudent(StudentId studentId)
    {
        RemoveStudentFromTutorialRollCommand command = new(TutorialId, RollId, studentId);

        _logger
            .ForContext(nameof(RemoveStudentFromTutorialRollCommand), command, true)
            .Information("Requested to remove student from Group Tutorial Roll by user {User}", _currentUserService.UserName);

        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, TutorialId, AuthPolicies.CanSubmitGroupTutorialRolls);

        if (!isAuthorised.Succeeded)
        {
            _logger
                .ForContext(nameof(Error), DomainErrors.Auth.NotAuthorised, true)
                .Warning("Failed to remove student from Group Tutorial Roll by user {User}", _currentUserService.UserName);

            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        Result result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), result.Error, true)
                .Warning("Failed to remove student from Group Tutorial Roll by user {User}", _currentUserService.UserName);

            return ShowError(result.Error);
        }

        return RedirectToPage();
    }

    private async Task PreparePage()
    {
        _logger.Information("Requested to retrieve Group Tutorial Roll with id {Id} by user {User}", RollId, _currentUserService.UserName);

        Result<TutorialRollDetailResponse> rollResult = await _mediator.Send(new GetTutorialRollWithDetailsByIdQuery(TutorialId, RollId));

        if (rollResult.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), rollResult.Error, true)
                .Warning("Failed to retrieve Group Tutorial Roll with id {Id} by user {User}", RollId, _currentUserService.UserName);
            
            ModalContent = ErrorDisplay.Create(
                rollResult.Error,
                _linkGenerator.GetPathByPage("/Subject/Tutorials/GroupTutorials/Index", values: new { area = "Staff" }));

            Roll = new(
                RollId,
                TutorialId,
                string.Empty,
                DateOnly.MinValue,
                StaffId.Empty,
                string.Empty,
                Core.Enums.TutorialRollStatus.Unsubmitted,
                new List<TutorialRollStudentResponse>());
        } 
        else
        {
            Roll = rollResult.Value;
            Students = rollResult.Value.Students.OrderBy(entry => entry.Grade).ThenBy(entry => entry.Name).ToList();
        }
    }

    private IActionResult ShowError(Error error)
    {
        ModalContent = ErrorDisplay.Create(
            error,
            _linkGenerator.GetPathByPage("/Subject/Tutorials/GroupTutorials/Roll", values: new { area = "Staff", TutorialId, RollId, Mode }));

        Roll = new(
            RollId,
            TutorialId,
            string.Empty,
            DateOnly.MinValue,
            StaffId.Empty,
            string.Empty,
            Core.Enums.TutorialRollStatus.Unsubmitted,
            new List<TutorialRollStudentResponse>());

        return Page();
    }
}
