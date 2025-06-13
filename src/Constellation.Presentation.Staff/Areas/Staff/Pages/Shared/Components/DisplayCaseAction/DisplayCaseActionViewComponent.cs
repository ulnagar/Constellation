namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.DisplayCaseAction;

using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Services;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.Models.WorkFlow;
using Constellation.Core.Models.WorkFlow.Identifiers;
using Constellation.Core.Models.WorkFlow.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class DisplayCaseActionViewComponent : ViewComponent
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ICaseRepository _caseRepository;

    public DisplayCaseActionViewComponent(
        ICurrentUserService currentUserService,
        IAuthorizationService authorizationService,
        ICaseRepository caseRepository)
    {
        _currentUserService = currentUserService;
        _authorizationService = authorizationService;
        _caseRepository = caseRepository;
    }

    public async Task<IViewComponentResult> InvokeAsync(CaseId caseId, ActionId actionId, bool showMenu)
    {
        Case? item = await _caseRepository.GetById(caseId);

        if (item is null)
            return Content(string.Empty);

        Action? action = item.Actions.FirstOrDefault(action => action.Id == actionId);

        if (action is null)
            return Content(string.Empty);

        List<Action> subActions = item.Actions.Where(entry => entry.ParentActionId == action.Id).ToList();

        StaffId staffId = _currentUserService.StaffId;

        bool assignedToMe = action.AssignedToId == staffId;

        bool assignedToMeInChain = action.AssignedToId == staffId ||
                            subActions.Any(subAction => subAction.AssignedToId == staffId);

        AuthorizationResult isAdmin = await _authorizationService.AuthorizeAsync(UserClaimsPrincipal, AuthPolicies.CanManageWorkflows);

        if (!assignedToMeInChain && !isAdmin.Succeeded)
            return Content(string.Empty);

        DisplayCaseActionViewModel viewModel = new()
        {
            Action = action,
            AssignedToMe = assignedToMe,
            IsAdmin = isAdmin.Succeeded,
            ShowMenu = showMenu
        };

        return View(viewModel);
    }
}