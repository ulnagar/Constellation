namespace Constellation.Presentation.Server.Areas.Admin.Pages.Configuration;

using Application.Models.Auth;
using BaseModels;
using Microsoft.AspNetCore.Mvc;
using Shared.Helpers.Attributes;

[HasPermission(AuthPermission.Admin_Configuration_Edit_Value)]
public class IndexModel : BasePageModel
{
    public IndexModel() { }

    [ViewData]
    public string ActivePage => Models.ActivePage.Configuration;
    
    public async Task OnGet() { }
}