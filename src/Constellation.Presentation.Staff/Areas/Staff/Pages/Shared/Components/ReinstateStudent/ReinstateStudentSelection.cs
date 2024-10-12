namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ReinstateStudent;

using Core.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class ReinstateStudentSelection
{
    public string SchoolCode { get; set; } = string.Empty;
    public Grade Grade { get; set; }

    public required SelectList SchoolList { get; set; }

}