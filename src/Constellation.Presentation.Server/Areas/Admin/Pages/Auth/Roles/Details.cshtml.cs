namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth.Roles;

using Application.Domains.Auth.Queries.GetRoleDetails;
using BaseModels;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Auth.Commands.AddUserToRole;
using Constellation.Application.Domains.Auth.Commands.RemoveUserFromRole;
using Constellation.Application.Models.Auth;
using Constellation.Core.Errors;
using Constellation.Presentation.Shared.Pages.Shared.Components.RoleAddUser;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

//[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class DetailsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Auth_Roles;
    [ViewData] public string PageTitle => "Auth Roles";
    
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public RoleDetailResponse RoleDetails { get; set; }

    public async Task OnGet()
    {
        Result<RoleDetailResponse> request = await _mediator.Send(new GetRoleDetailsQuery(Id));

        if (request.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/Auth/Roles/Index", values: new { area = "Admin" }));

            return;
        }

        RoleDetails = request.Value;
    }

    public async Task<IActionResult> OnGetRemoveUser(Guid userId)
    {
        if (userId == Guid.Empty)
            return ShowError(DomainErrors.Auth.UserNotFound);

        Result result = await _mediator.Send(new RemoveUserFromRoleCommand(Id, userId));

        if (result.IsFailure)
            return ShowError(result.Error);

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddUser(RoleAddUserSelection viewModel)
    {
        if (viewModel.UserId == Guid.Empty)
            return ShowError(DomainErrors.Auth.UserNotFound);

        Result result = await _mediator.Send(new AddUserToRoleCommand(viewModel.RoleId, viewModel.UserId));

        if (result.IsFailure)
            return ShowError(result.Error);

        return RedirectToPage();
    }

    private IActionResult ShowError(Error error)
    {
        ModalContent = ErrorDisplay.Create(
            error,
            _linkGenerator.GetPathByPage("/Auth/Roles/Index", values: new { area = "Admin" }));
        
        return Page();
    }
}
