namespace Constellation.Presentation.Parents.Areas.Parents.Pages;

using Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

[Authorize(Policy = AuthPolicies.IsParent)]
public class IndexModel : BasePageModel
{

    public IndexModel()
    {
        
    }

    [ViewData] public string ActivePage => Models.ActivePage.Dashboard;

    public async Task OnGet()
    {

    }
}
