namespace Constellation.Presentation.Shared.Pages.Shared.Components.PageFooter;

using Constellation.Application.Interfaces.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Reflection;

public class PageFooterViewComponent : ViewComponent
{
    private readonly AppConfiguration _configuration;

    public PageFooterViewComponent(
        IOptions<AppConfiguration> configuration)
    {
        _configuration = configuration.Value;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        string file = Assembly.GetExecutingAssembly().Location;
        DateTime lastWriteTime = File.GetLastWriteTime(file);

        PageFooterViewModel viewModel = new()
        {
            VersionLabel = $"{lastWriteTime:yyMM.d}-{_configuration.DebugLabel}",
            Year = DateTime.Today.Year.ToString()
        };

        return View(viewModel);
    }
}