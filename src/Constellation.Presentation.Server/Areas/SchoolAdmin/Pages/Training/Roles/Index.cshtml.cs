namespace Constellation.Presentation.Server.Areas.SchoolAdmin.Pages.Training.Roles;

using Application.Models.Auth;
using Application.Training.Roles.DeleteTrainingRole;
using Application.Training.Roles.GetTrainingRoleList;
using BaseModels;
using Constellation.Application.Training.Models;
using Core.Errors;
using Core.Models.Training.Identifiers;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Policy = AuthPolicies.IsStaffMember)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly IAuthorizationService _authService;

    public IndexModel(
        ISender mediator,
        IAuthorizationService authService)
    {
        _mediator = mediator;
        _authService = authService;
    }

    public List<TrainingRoleResponse> Roles { get; set; }

    [ViewData] public string ActivePage { get; set; } = TrainingPages.Roles;
    [ViewData] public string StaffId { get; set; }

    public async Task OnGet()
    {
        StaffId = User.Claims.First(claim => claim.Type == AuthClaimType.StaffEmployeeId)?.Value;

        await GetClasses(_mediator);

        Result<List<TrainingRoleResponse>> request = await _mediator.Send(new GetTrainingRoleListQuery());

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return;
        }

        Roles = request.Value;
    }

    public async Task<IActionResult> OnGetDelete(Guid Id)
    {
        AuthorizationResult canEdit = await _authService.AuthorizeAsync(User, AuthPolicies.CanEditTrainingModuleContent);

        if (!canEdit.Succeeded)
        {
            Error = new()
            {
                Error = DomainErrors.Auth.NotAuthorised,
                RedirectPath = null
            };

            return Page();
        }

        TrainingRoleId roleId = TrainingRoleId.FromValue(Id);

        Result request = await _mediator.Send(new DeleteTrainingRoleCommand(roleId));

        if (request.IsFailure)
        {
            Error = new()
            {
                Error = request.Error,
                RedirectPath = null
            };

            return Page();
        }

        return RedirectToPage();
    }
}