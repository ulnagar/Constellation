namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.PartialViews.DashboardWidget;

using System.Collections.Generic;

public enum WidgetVariant { Neutral, Success, Danger }

public abstract record DashboardWidgetModel(
    string Id,
    string Title,
    int ColSpan = 1,
    int RowSpan = 1)
{
    public virtual WidgetVariant Variant => WidgetVariant.Neutral;
}

public sealed record CountWidgetModel(
    string Id,
    string Title,
    int Count,
    string Description,
    string Page,
    string Area = "Staff",
    string? CountDisplay = null,
    int WarningThreshold = 0,
    int ColSpan = 1,
    int RowSpan = 1,
    IDictionary<string, string>? RouteValues = null)
    : DashboardWidgetModel(Id, Title, ColSpan, RowSpan)
{
    public override WidgetVariant Variant => Count > WarningThreshold 
        ? WidgetVariant.Danger 
        : WidgetVariant.Success;
}

public sealed record ChartWidgetModel(
    string Id,
    string Title,
    string ChartTitle,
    IReadOnlyList<string> Labels,
    IReadOnlyList<int> Values,
    string ChartType = "bar",
    int ColSpan = 2, 
    int RowSpan = 1)
    : DashboardWidgetModel(Id, Title, ColSpan, RowSpan);