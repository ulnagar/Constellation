namespace Constellation.Presentation.Server.Areas.Admin.Pages;

using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Authorization;

[AllowAnonymous]
public class AccessDeniedModel : BasePageModel
{
    public string ReturnUrl { get; set; }

    public void OnGet(string returnUrl = null) => ReturnUrl = returnUrl;
}