namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ReinstateStudent;

using Application.Schools.Models;
using Core.Enums;

public sealed class ReinstateStudentSelection
{
    public string SchoolCode { get; set; } = string.Empty;
    public Grade Grade { get; set; }

    public List<SchoolSelectionListResponse> Schools { get; set; } = new();
}