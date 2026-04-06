namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.BulkCancelSciencePracRolls;

using Application.Domains.Schools.Models;
using Core.Enums;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed class BulkCancelSciencePracRollsSelection
{
    public List<SchoolSelectionListResponse> Schools { get; set; }

    public List<SchoolCode> SelectedSchoolCodes { get; set; } = new();
    public List<Grade> SelectedGrades { get; set; } = new();
    public string Comment { get; set; } = string.Empty;
}
