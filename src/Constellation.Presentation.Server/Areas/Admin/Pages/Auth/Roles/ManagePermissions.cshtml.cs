namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth.Roles;

using Application.Common.PresentationModels;
using Application.Domains.Auth.Commands.AddPermissionToRole;
using Application.Domains.Auth.Queries.GetRoleDetails;
using Application.Models.Auth;
using BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Admin_Authentication_Edit_Value)]
public class ManagePermissionsModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public ManagePermissionsModel(
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Auth_Roles;
    [ViewData] public string PageTitle => "Auth Roles";

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; } = Guid.Empty;

    [BindProperty]
    public List<AuthPermission> Permissions { get; set; } = [];

    public string RoleName { get; set; }
    public List<AuthPermission> EnabledPermissions { get; set; } = [];

    public List<AuthPermission> AvailablePermissions { get; set; } = [];

    public async Task OnGet()
    {
        AvailablePermissions = AuthPermission.GetOptions.ToList();

        Result<RoleDetailResponse> request = await _mediator.Send(new GetRoleDetailsQuery(Id));

        if (request.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                request.Error,
                _linkGenerator.GetPathByPage("/Auth/Roles/Index", values: new { area = "Admin" }));

            return;
        }

        RoleName = request.Value.Name;
        EnabledPermissions = request.Value.Permissions;
    }

    public async Task<IActionResult> OnPost()
    {
        if (Permissions.Count == 0)
        {
            ModalContent = FeedbackDisplay.Create(
                "Permissions",
                "No permissions selected. Cannot update Role.",
                "Ok",
                "btn-primary",
                _linkGenerator.GetPathByPage("/Auth/Roles/Details", values: new { area = "Admin", Id }));

            return Page();
        }

        foreach (AuthPermission permission in Permissions)
        {
            await _mediator.Send(new AddPermissionToRoleCommand(Id, permission));
        }

        return RedirectToPage("/Auth/Roles/Details", routeValues: new { area = "Admin", Id });
    }
}