namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.PeriodSwitcher;

using Constellation.Core.Models.EnrolmentContext.EnrolmentPeriod.Identifiers;

public sealed class PeriodSwitcherViewModel
{
    public PeriodSwitcherViewModel(
        List<PeriodSwitcherOption> options)
    {
        Periods = options;
    }

    public List<PeriodSwitcherOption> Periods { get; set; } = [];
};

public sealed class PeriodSwitcherOption
{
    public required EnrolmentPeriodId PeriodId { get; set; }
    public required string Label { get; set; }
    public required bool IsCurrent { get; set; }
    public required bool CurrentlySelected { get; set; }
    public required string Url { get; set; }
}