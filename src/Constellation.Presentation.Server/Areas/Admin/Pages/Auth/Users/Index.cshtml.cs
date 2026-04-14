namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth.Users;

using Application.Common.PresentationModels;
using Application.Domains.Auth.Commands.AuditAllUsers;
using Application.Domains.Auth.Commands.AuditUser;
using Application.Domains.Auth.Queries.GetFilteredUsers;
using Application.Models.Identity.Errors;
using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Infrastructure.Identity.Authorization;
using Constellation.Presentation.Server.BaseModels;
using Core.Models.Auth;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Admin_Authentication_View_Value)]
public class IndexModel : BasePageModel
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMediator _mediator;
    private readonly LinkGenerator _linkGenerator;
    
    public IndexModel(
        IAuthorizationService authorizationService,
        IMediator mediator,
        LinkGenerator linkGenerator)
    {
        _authorizationService = authorizationService;
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Models.ActivePage.Auth_Users;
    [ViewData] public string PageTitle => "Auth Users";

    public List<AppUser> Users { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public UserFilter Filter { get; set; } = UserFilter.Staff;
    
    public async Task OnGet()
    {
        await PreparePage();
    }
    
    public async Task<IActionResult> OnGetAudit(Guid userId)
    {
        IAuthorizationRequirement permissionRequirement = new PermissionRequirement([AuthPermission.Admin_Authentication_Edit]);

        AuthorizationResult canEdit = await _authorizationService.AuthorizeAsync(User, null, permissionRequirement);

        if (!canEdit.Succeeded)
        {
            ModalContent = ErrorDisplay.Create(AuthErrors.NotAuthorised);

            await PreparePage();
        }

        Result result = await _mediator.Send(new AuditUserCommand(userId));

        if (result.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                result.Error,
                _linkGenerator.GetPathByPage("/Auth/Users/Index", values: new { area = "Admin" }));

            await PreparePage();
        }

        return RedirectToPage();
    }

    public async Task OnGetAuditAllUsers(CancellationToken cancellationToken = default)
    {
        IAuthorizationRequirement permissionRequirement = new PermissionRequirement([AuthPermission.Admin_Authentication_Edit]);

        AuthorizationResult canEdit = await _authorizationService.AuthorizeAsync(User, null, permissionRequirement);

        if (!canEdit.Succeeded)
        {
            ModalContent = ErrorDisplay.Create(AuthErrors.NotAuthorised);

            await PreparePage();
        }

        await _mediator.Send(new AuditAllUsersCommand(), cancellationToken);
    }


    private async Task<IActionResult> PreparePage()
    {
        Result<List<AppUser>> users = await _mediator.Send(new GetFilteredUsersQuery(Filter));

        if (users.IsFailure)
        {
            return Page();
        }

        Users = users.Value;

        return Page();
    }
}
