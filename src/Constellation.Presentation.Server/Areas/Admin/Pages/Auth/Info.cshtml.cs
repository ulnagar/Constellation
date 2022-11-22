using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth;

public class InfoModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICurrentUserService _currentUserService;

    public InfoModel(IMediator mediator, UserManager<AppUser> userManager,
        ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _userManager = userManager;
        _currentUserService = currentUserService;
    }

    public List<Claim> Claims { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public string CurrentUserServiceName { get; set; }

    public async Task OnGetAsync()
    {
        await GetClasses(_mediator);

        CurrentUserServiceName = _currentUserService.UserName;

        Claims = User.Claims.ToList();

        if (User.Identity is not null)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            Roles = (List<string>)await _userManager.GetRolesAsync(user);
        }
    }
}
