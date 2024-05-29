namespace Constellation.Presentation.Server.Areas.Subject.Pages.GroupTutorials.Tutorials;

using Application.Common.PresentationModels;
using Constellation.Application.GroupTutorials.AddStudentToRoll;
using Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;
using Constellation.Application.GroupTutorials.RemoveStudentFromTutorialRoll;
using Constellation.Application.GroupTutorials.SubmitRoll;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using Constellation.Presentation.Server.BaseModels;
using Core.Abstractions.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Pages.Shared.Components.TutorialRollAddStudent;

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

    [ViewData] public string ActivePage => SubjectPages.Tutorials;

    [BindProperty(SupportsGet = true)]
    public Guid TutorialId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid RollId { get; set; }

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
            var isAuthorised = await _authorizationService.AuthorizeAsync(User, TutorialId, AuthPolicies.CanSubmitGroupTutorialRolls);

            if (!isAuthorised.Succeeded)
            {
                return ShowError(DomainErrors.Permissions.Unauthorised);
            }
        }

        await GetPageInformation();

        return Page();
    }

    public async Task<IActionResult> OnPostSubmit()
    {
        AuthorizationResult isAuthorised = await _authorizationService.AuthorizeAsync(User, TutorialId, AuthPolicies.CanSubmitGroupTutorialRolls);

        if (!isAuthorised.Succeeded)
            return ShowError(DomainErrors.Permissions.Unauthorised);

        string emailAddress = _currentUserService.EmailAddress;

        Dictionary<string, bool> studentData = Students.ToDictionary(k => k.StudentId, k => k.Present);

        Result result = await _mediator.Send(new SubmitRollCommand(
            GroupTutorialId.FromValue(TutorialId),
            TutorialRollId.FromValue(RollId),
            emailAddress,
            studentData));

        if (result.IsFailure)
            return ShowError(result.Error);

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

        var result = await _mediator.Send(new AddStudentToTutorialRollCommand(GroupTutorialId.FromValue(TutorialId), TutorialRollId.FromValue(RollId), AddStudentSelection.StudentId));

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

        var result = await _mediator.Send(new RemoveStudentFromTutorialRollCommand(GroupTutorialId.FromValue(TutorialId), TutorialRollId.FromValue(RollId), studentId));

        if (result.IsFailure)
        {
            return ShowError(result.Error);
        }

        return RedirectToPage("Roll", new { TutorialId, RollId, Mode });
    }

    private async Task GetPageInformation()
    {
        var rollResult = await _mediator.Send(new GetTutorialRollWithDetailsByIdQuery(GroupTutorialId.FromValue(TutorialId), TutorialRollId.FromValue(RollId)));

        if (rollResult.IsFailure)
        {
            Error = new ErrorDisplay
            {
                Error = rollResult.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/GroupTutorials/Tutorials/Index", values: new { area = "Subject" })
            };

            Roll = new(
                TutorialRollId.FromValue(RollId),
                GroupTutorialId.FromValue(TutorialId),
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
            Students = rollResult.Value.Students.OrderBy(entry => entry.Grade).ThenBy(entry => entry.Name).ToList();
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
            TutorialRollId.FromValue(RollId),
            GroupTutorialId.FromValue(TutorialId),
            string.Empty,
            DateOnly.MinValue,
            string.Empty,
            string.Empty,
            Core.Enums.TutorialRollStatus.Unsubmitted,
            new List<TutorialRollStudentResponse>());

        return Page();
    }
}
