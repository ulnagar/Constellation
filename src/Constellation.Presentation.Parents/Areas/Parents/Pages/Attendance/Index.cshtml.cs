namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Attendance;

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

    [ViewData] public string ActivePage => Models.ActivePage.Attendance;

    public void OnGet()
    {
    }
}