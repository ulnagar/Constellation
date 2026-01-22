namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth.Roles;

using Application.Domains.Auth.Commands.AddPermissionToRole;
using Application.Domains.Auth.Commands.RemovePermissionFromRole;
using Application.Domains.Auth.Queries.GetRoleDetails;
using Application.Models.Auth;
using BaseModels;
using Constellation.Application.Common.PresentationModels;
using Constellation.Application.Domains.Auth.Commands.AddUserToRole;
using Constellation.Application.Domains.Auth.Commands.RemoveUserFromRole;
using Constellation.Core.Errors;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Constellation.Presentation.Shared.Pages.Shared.Components.RoleAddUser;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers.ModelBinders;
using Shared.Pages.Shared.Components.RoleAddPermission;

[HasPermission(AuthPermission.Admin_Authentication_View_Value)]
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
        {
            ModalContent = ErrorDisplay.Create(
                DomainErrors.Auth.UserNotFound,
                _linkGenerator.GetPathByPage("/Auth/Roles/Details", values: new { area = "Admin", Id }));

            return Page();
        }

        Result result = await _mediator.Send(new RemoveUserFromRoleCommand(Id, userId));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Auth/Roles/Details", values: new { area = "Admin", Id }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddUser(RoleAddUserSelection viewModel)
    {
        if (viewModel.UserId == Guid.Empty)
        {
            ModalContent = ErrorDisplay.Create(
                DomainErrors.Auth.UserNotFound,
                _linkGenerator.GetPathByPage("/Auth/Roles/Details", values: new { area = "Admin", Id }));

            return Page();
        }

        Result result = await _mediator.Send(new AddUserToRoleCommand(Id, viewModel.UserId));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Auth/Roles/Details", values: new { area = "Admin", Id }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddPermission(RoleAddPermissionSelection viewModel)
    {
        Result result = await _mediator.Send(new AddPermissionToRoleCommand(Id, viewModel.Permission));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Auth/Roles/Details", values: new { area = "Admin", Id }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnGetRemovePermission([ModelBinder(typeof(BaseFromValueBinder))] AuthPermission permission)
    {
        Result result = await _mediator.Send(new RemovePermissionFromRoleCommand(Id, permission));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Auth/Roles/Details", values: new { area = "Admin", Id }));

            return Page();
        }

        return RedirectToPage();
    }
}
