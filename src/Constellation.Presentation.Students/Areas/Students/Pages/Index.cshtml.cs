namespace Constellation.Presentation.Students.Areas.Students.Pages;

using Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;

[Authorize(Policy = AuthPolicies.IsStudent)]
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
