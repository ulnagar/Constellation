namespace Constellation.Presentation.Server.Pages.Auth;

using BaseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

[AllowAnonymous]
public class AccessDeniedSSOModel : BasePageModel
{
    public AccessDeniedSSOModel()
    {
        
    }

    public async Task OnGet()
    {
    }
}