﻿namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Reports;

using Application.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
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
