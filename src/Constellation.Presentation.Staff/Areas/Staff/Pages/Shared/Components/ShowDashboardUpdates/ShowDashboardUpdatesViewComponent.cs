namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ShowDashboardUpdates;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

public sealed class ShowDashboardUpdatesViewComponent : ViewComponent
{
    private readonly IWebHostEnvironment _environment;

    public ShowDashboardUpdatesViewComponent(
        IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        ShowDashboardUpdatesViewComponentModel viewModel = new();

        var filepath = _environment.ContentRootPath;

        using StreamReader r = new(Path.Combine(filepath, "RecentUpdates.json"));
        string json = await r.ReadToEndAsync(cancellationToken);
        List<ShowDashboardUpdatesViewComponentModel.RecentChange>? updates = JsonConvert.DeserializeObject<List<ShowDashboardUpdatesViewComponentModel.RecentChange>>(json);
        viewModel.Changes = updates ?? [];

        return View(viewModel);
    }
}