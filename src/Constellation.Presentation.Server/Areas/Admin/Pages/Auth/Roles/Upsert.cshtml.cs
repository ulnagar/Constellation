namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth.Roles;

using Application.Common.PresentationModels;
using Application.Domains.Auth.Commands.CreateRole;
using Application.Domains.Auth.Commands.UpdateRole;
using Application.Domains.Auth.Queries.GetRoleDetails;
using Application.Models.Auth;
using Application.Models.Identity.Enums;
using BaseModels;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers.Attributes;
using Shared.Helpers.ModelBinders;

[HasPermission(AuthPermission.Admin_Authentication_Edit_Value)]
public class UpsertModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;

    public UpsertModel(
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
    public string Name { get; set; }

    [BindProperty]
    [ModelBinder(typeof(BaseFromValueBinder))]
    public AppRoleType Type { get; set; }

    public List<AppRoleType> RoleTypes { get; set; }

    public async Task OnGet()
    {
        RoleTypes = AppRoleType.GetOptions.ToList();

        if (Id != Guid.Empty)
        {
            Result<RoleDetailResponse> role = await _mediator.Send(new GetRoleDetailsQuery(Id));

            if (role.IsFailure)
            {
                ModalContent = ErrorDisplay.Create(
                    role.Error,
                    _linkGenerator.GetPathByPage("/Auth/Roles/Index", values: new { area = "Admin" }));

                return;
            }

            Name = role.Value.Name;
            Type = role.Value.Type;
        }
    }

    public async Task<IActionResult> OnPost()
    {
        Result<Guid> result = Id == Guid.Empty
            ? await _mediator.Send(new CreateRoleCommand(Name, Type))
            : await _mediator.Send(new UpdateRoleCommand(Id, Name, Type));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Auth/Roles/Index", values: new { area = "Admin" }));

            return Page();
        }

        return RedirectToPage("/Auth/Roles/Details", routeValues: new { area = "Admin", Id = result.Value });
    }
}