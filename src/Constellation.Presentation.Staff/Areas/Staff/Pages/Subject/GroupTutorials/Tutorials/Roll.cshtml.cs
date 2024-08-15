namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Subject.GroupTutorials.Tutorials;

using Constellation.Application.Common.PresentationModels;
using Constellation.Application.GroupTutorials.AddStudentToRoll;
using Constellation.Application.GroupTutorials.GetTutorialRollWithDetailsById;
using Constellation.Application.GroupTutorials.RemoveStudentFromTutorialRoll;
using Constellation.Application.GroupTutorials.SubmitRoll;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Errors;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Components.TutorialRollAddStudent;

[Authorize(Policy = AuthPolicies.CanViewGroupTutorials)]
public class RollModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly LinkGenerator _linkGenerator;
    private readonly IAuthorizationService _authorizationService;

    public RollModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        LinkGenerator linkGenerator,
        IAuthorizationService authorizationService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _linkGenerator = linkGenerator;
        _authorizationService = authorizationService;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Subject_GroupTutorials_Tutorials;

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

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Details", new { area = "Staff", Id = TutorialId });
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

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Roll", new { area = "Staff", TutorialId, RollId, Mode });
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

        return RedirectToPage("/Subject/GroupTutorials/Tutorials/Roll", new { area = "Staff", TutorialId, RollId, Mode });
    }

    private async Task GetPageInformation()
    {
        var rollResult = await _mediator.Send(new GetTutorialRollWithDetailsByIdQuery(GroupTutorialId.FromValue(TutorialId), TutorialRollId.FromValue(RollId)));

        if (rollResult.IsFailure)
        {
            ModalContent = new ErrorDisplay(
                rollResult.Error,
                _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Index", values: new { area = "Staff" }));

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
        ModalContent = new ErrorDisplay(
            error,
            _linkGenerator.GetPathByPage("/Subject/GroupTutorials/Tutorials/Roll", values: new { area = "Staff", TutorialId, RollId, Mode }));

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
