namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ReinstateStudent;

using Core.Enums;
using Core.Models.Identifiers;
using Microsoft.AspNetCore.Mvc.Rendering;

public sealed class ReinstateStudentSelection
{
    public SchoolCode SchoolCode { get; set; } = SchoolCode.Empty;
    public Grade Grade { get; set; }

    public required SelectList SchoolList { get; set; }

}