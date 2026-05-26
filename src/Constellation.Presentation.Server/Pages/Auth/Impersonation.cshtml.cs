namespace Constellation.Presentation.Server.Pages.Auth;

using Application.Interfaces.Services;
using Application.Models.Auth;
using BaseModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class ImpersonationModel : BasePageModel
{
    private readonly IAuthService _authService;
    private readonly IAuthorizationService _authorizationService;

    public ImpersonationModel(
        IAuthService authService,
        IAuthorizationService authorizationService)
    {
        _authService = authService;
        _authorizationService = authorizationService;
    }

    [BindProperty(SupportsGet = true)]
    public Guid TargetUserId { get; set; } = Guid.Empty;

    public async Task<IActionResult> OnPostImpersonate()
    {
        if (TargetUserId == Guid.Empty)
            return BadRequest();

        AuthorizationResult isAdmin = await _authorizationService.AuthorizeAsync(User, AuthPolicies.IsSiteAdmin);

        if (!isAdmin.Succeeded)
            return Forbid();

        ImpersonationResult result = await _authService.ImpersonateAsync(TargetUserId, User);

        if (!result.Succeeded)
            throw new AuthenticationFailureException(result.ErrorMessage);

        // Drop straight into the Parents area as the target user
        return RedirectToPage("/Index");
    }

    public async Task<IActionResult> OnPostStopImpersonating()
    {
        ImpersonationResult result = await _authService.StopImpersonatingAsync(User);

        if (!result.Succeeded)
            throw new AuthenticationFailureException(result.ErrorMessage);

        return RedirectToPage("/Index");
    }
}