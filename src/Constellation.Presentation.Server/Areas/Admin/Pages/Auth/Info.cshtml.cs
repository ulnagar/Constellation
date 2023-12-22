namespace Constellation.Presentation.Server.Areas.Admin.Pages.Auth;

using Constellation.Application.Models.Auth;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize(Policy = AuthPolicies.IsSiteAdmin)]
public class InfoModel : BasePageModel
{
    private readonly IMediator _mediator;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;

    public InfoModel(IMediator mediator, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _mediator = mediator;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty(SupportsGet = true)]
    public string EmailAddress { get; set; }
    public AppUser AppUser { get; set; }
    public List<Claim> Claims { get; set; } = new();
    public List<string> Roles { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        if (string.IsNullOrWhiteSpace(EmailAddress))
        {
            return RedirectToPage("Index");    
        }

        AppUser = await _userManager.FindByEmailAsync(EmailAddress);

        Claims = (await _userManager.GetClaimsAsync(AppUser)).ToList();

        Roles = (await _userManager.GetRolesAsync(AppUser)).ToList();

        foreach (var roleName in Roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            Claims.AddRange(await _roleManager.GetClaimsAsync(role));
        }

        return Page();
    }
}
