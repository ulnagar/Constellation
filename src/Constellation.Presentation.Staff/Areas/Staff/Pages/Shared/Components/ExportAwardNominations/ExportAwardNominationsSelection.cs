namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ExportAwardNominations;

using Application.Awards.ExportAwardNominations;

public sealed class ExportAwardNominationsSelection
{
    public bool ShowClass { get; set; }
    public bool ShowGrade { get; set; }
    public ExportAwardNominationsCommand.GroupCategory Grouping { get; set; }
}
