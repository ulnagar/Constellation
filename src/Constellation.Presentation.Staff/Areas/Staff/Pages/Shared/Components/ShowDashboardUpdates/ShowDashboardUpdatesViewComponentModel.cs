namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ShowDashboardUpdates;

public sealed class ShowDashboardUpdatesViewComponentModel
{
    public List<RecentChange> Changes { get; set; } = new();

    public sealed class RecentChange
    {
        public DateTime Datestamp { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Type { get; set; } = "dark";
    }
}