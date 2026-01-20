namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth.Users;

using Application.Common.PresentationModels;
using Application.Domains.Auth.Queries.GetUserDetails;
using Application.Models.Identity.Errors;
using Constellation.Application.Domains.Auth.Commands.AddUserToRole;
using Constellation.Application.Domains.Auth.Commands.RemoveUserFromRole;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers.Attributes;
using Shared.Pages.Shared.Components.UserAddRole;

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

    [ViewData] public string ActivePage => Models.ActivePage.Auth_Users;
    [ViewData] public string PageTitle => "Auth Users";


    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    public UserResponse User { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (Id == Guid.Empty)
            return RedirectToPage("/Auth/Users/Index", routeValues: new { area = "Admin" });

        await PreparePage();

        return Page();
    }

    public async Task<IActionResult> OnGetRemoveRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
        {
            await PreparePage();

            return Page();
        }

        Result result = await _mediator.Send(new RemoveUserFromRoleCommand(roleId, Id));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Auth/Users/Details", values: new { area = "Admin", Id }));

            await PreparePage();

            return Page();
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddRole(UserAddRoleSelection viewModel)
    {
        if (viewModel.RoleId == Guid.Empty)
        {
            ModalContent = ErrorDisplay.Create(
                AuthErrors.RoleNotFound(Guid.Empty),
                _linkGenerator.GetPathByPage("/Auth/Users/Details", values: new { area = "Admin", Id }));

            return Page();
        }

        Result result = await _mediator.Send(new AddUserToRoleCommand(viewModel.RoleId, Id));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Auth/Users/Details", values: new { area = "Admin", Id }));

            return Page();
        }

        return RedirectToPage();
    }

    public async Task PreparePage()
    {
        Result<UserResponse> user = await _mediator.Send(new GetUserDetailsQuery(Id));

        if (user.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                user.Error,
                _linkGenerator.GetPathByPage("/Auth/Users/Index", values: new { area = "Admin" }));

            return;
        }

        User = user.Value;
    }
}
