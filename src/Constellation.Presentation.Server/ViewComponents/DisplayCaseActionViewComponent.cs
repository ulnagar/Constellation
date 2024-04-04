﻿namespace Constellation.Presentation.Server.ViewComponents;

using Application.Models.Auth;
using Core.Abstractions.Services;
using Core.Models.WorkFlow;
using Core.Models.WorkFlow.Identifiers;
using Core.Models.WorkFlow.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pages.Shared.Components.DisplayCaseAction;

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

    public async Task<IViewComponentResult> InvokeAsync(Guid caseId, Guid actionId)
    {
        Case item = await _caseRepository.GetById(CaseId.FromValue(caseId));

        if (item is null)
            return Content(string.Empty);

        Action action = item.Actions.FirstOrDefault(action => action.Id == ActionId.FromValue(actionId));

        if (action is null)
            return Content(string.Empty);

        List<Action> subActions = item.Actions.Where(entry => entry.ParentActionId == action.Id).ToList();

        string currentUserId = UserClaimsPrincipal.Claims.FirstOrDefault(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        bool assignedToMe = action.AssignedToId == currentUserId;

        bool assignedToMeInChain = action.AssignedToId == currentUserId ||
                            subActions.Any(subAction => subAction.AssignedToId == currentUserId);

        AuthorizationResult isAdmin = await _authorizationService.AuthorizeAsync(UserClaimsPrincipal, AuthPolicies.CanManageWorkflows);

        if (!assignedToMeInChain && !isAdmin.Succeeded)
            return Content(string.Empty);

        DisplayCaseActionViewModel viewModel = new()
        {
            Action = action, 
            AssignedToMe = assignedToMe, 
            IsAdmin = isAdmin.Succeeded
        };

        return View(viewModel);
    }
}