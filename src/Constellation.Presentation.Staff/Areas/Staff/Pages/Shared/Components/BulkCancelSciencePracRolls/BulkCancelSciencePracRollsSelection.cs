namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.BulkCancelSciencePracRolls;

using Application.Schools.Models;
using Core.Enums;
using System.Collections.Generic;

public sealed class BulkCancelSciencePracRollsSelection
{
    public List<SchoolSelectionListResponse> Schools { get; set; }

    public List<string> SelectedSchoolCodes { get; set; } = new();
    public List<Grade> SelectedGrades { get; set; } = new();
    public string Comment { get; set; } = string.Empty;
}
