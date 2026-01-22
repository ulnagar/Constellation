namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Reports;

using Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[HasPermission(AuthPermission.Equipment_Assets_Edit_Value)]
public class IndexModel : BasePageModel
{
    public IndexModel()
    {
        
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Assets_Reports;
    [ViewData] public string PageTitle => "Asset Reports";

    public async Task OnGet()
    {

    }
}
