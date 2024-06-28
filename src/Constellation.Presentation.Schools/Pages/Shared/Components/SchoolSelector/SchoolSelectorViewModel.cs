namespace Constellation.Presentation.Schools.Pages.Shared.Components.SchoolSelector;

using Application.DTOs;
using System.Collections.Generic;

public sealed class SchoolSelectorViewModel
{
    public List<SchoolDto> ValidSchools { get; set; }

    public string CurrentSchoolCode { get; set; }

}
