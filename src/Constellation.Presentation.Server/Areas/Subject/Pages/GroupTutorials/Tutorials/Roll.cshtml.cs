namespace Constellation.Presentation.Server.Areas.Subject.Pages.GroupTutorials.Tutorials;

using Constellation.Application.GroupTutorials.AddStudentToRoll;
using Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;
using Constellation.Application.GroupTutorials.RemoveStudentFromTutorialRoll;
using Constellation.Application.GroupTutorials.SubmitRoll;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Pages.Shared.Components.TutorialRollAddStudent;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

[Authorize(Policy = AuthPolicies.CanViewGroupTutorials)]
public class RollModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;

    public RollModel(
        IMediator mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid TutorialId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid RollId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Mode { get; set; }

    [BindProperty]
    public List<TutorialRollStudentResponse> Students { get; set; }

    public TutorialRollDetailResponse Roll { get; set; }

    [BindProperty]
    public TutorialRollAddStudentSelection AddStudentSelection { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (Mode == "Edit")
        {
            var isAuthorised = await _authorizationService.AuthorizeAsync(User, TutorialId, AuthPolicies.CanSubmitGroupTutorialRolls);

            if (!isAuthorised.Succeeded)
            {
                return ShowError(DomainErrors.Permissions.Unauthorised);
            }
        }

        await GetClasses(_mediator);

        await GetPageInformation();

        return Page();
    }

    public async Task<IActionResult> OnPostSubmit()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, TutorialId, AuthPolicies.CanSubmitGroupTutorialRolls);

        if (!isAuthorised.Succeeded)
        {
            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        var username = _currentUserService.UserName;

        var result = await _mediator.Send(new SubmitRollCommand(
            TutorialId,
            RollId,
            username,
            Students.ToDictionary(k => k.StudentId, k => k.Present)));

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        return RedirectToPage("Details", new { Id = TutorialId });
    }

    public async Task<IActionResult> OnPostAddStudent()
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, TutorialId, AuthPolicies.CanSubmitGroupTutorialRolls);

        if (!isAuthorised.Succeeded)
        {
            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        if (string.IsNullOrWhiteSpace(AddStudentSelection.StudentId))
        {
            await GetPageInformation();
            return Page();
        }

        var result = await _mediator.Send(new AddStudentToTutorialRollCommand(TutorialId, RollId, AddStudentSelection.StudentId));

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        AddStudentSelection = new();

        return RedirectToPage("Roll", new { TutorialId, RollId, Mode });
    }

    public async Task<IActionResult> OnGetRemoveStudent(string studentId)
    {
        var isAuthorised = await _authorizationService.AuthorizeAsync(User, TutorialId, AuthPolicies.CanSubmitGroupTutorialRolls);

        if (!isAuthorised.Succeeded)
        {
            return ShowError(DomainErrors.Permissions.Unauthorised);
        }

        var result = await _mediator.Send(new RemoveStudentFromTutorialRollCommand(TutorialId, RollId, studentId));

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        return RedirectToPage("Roll", new { TutorialId, RollId, Mode });
    }

    private async Task GetPageInformation()
    {
        var rollResult = await _mediator.Send(new GetTutorialRollWithDetailsByIdQuery(TutorialId, RollId));

        if (rollResult.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = rollResult.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/GroupTutorials/Tutorials/Roll", values: new { area = "Subject", TutorialId, RollId, Mode })
            };

            Roll = new(
                RollId,
                TutorialId,
                string.Empty,
                DateOnly.MinValue,
                string.Empty,
                string.Empty,
                Core.Enums.TutorialRollStatus.Unsubmitted,
                new List<TutorialRollStudentResponse>());
        } 
        else
        {
            Roll = rollResult.Value;
            Students = rollResult.Value.Students.ToList();
        }
    }

    private IActionResult ShowError(Error error)
    {
        Error = new ErrorDisplay
        {
            Error = error,
            RedirectPath = _linkGenerator.GetPathByPage("/GroupTutorials/Tutorials/Roll", values: new { area = "Subject", TutorialId, RollId, Mode })
        };

        Roll = new(
            RollId,
            TutorialId,
            string.Empty,
            DateOnly.MinValue,
            string.Empty,
            string.Empty,
            Core.Enums.TutorialRollStatus.Unsubmitted,
            new List<TutorialRollStudentResponse>());

        return Page();
    }
}
