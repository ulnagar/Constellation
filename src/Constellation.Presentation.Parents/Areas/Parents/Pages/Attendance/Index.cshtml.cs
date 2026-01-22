namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Attendance;

using Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using Models;

[HasPermission(AuthPermission.ParentPortal_View_Value)]
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