namespace Constellation.Presentation.Parents.Areas.Parents.Pages.Shared.Components.ParentNav;

using Application.Interfaces.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

public class ParentNavViewComponent : ViewComponent
{
    private readonly IOptions<ParentPortalConfiguration> _configuration;

    public ParentNavViewComponent(
        IOptions<ParentPortalConfiguration> configuration)
    {
        _configuration = configuration;
    }

    public IViewComponentResult Invoke(string activePage)
    {
        ParentNavViewModel viewModel = new()
        {
            ActivePage = activePage,
            ShowConsent = _configuration.Value.ShowConsent
        };
        
        return View("ParentNav", viewModel);
    }
}