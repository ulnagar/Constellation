namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Shared.Components.SchoolSelector;

using Constellation.Application.Schools.GetSchoolsForContact;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

public sealed class SchoolSelectorViewModel
{
    public List<SchoolResponse> ValidSchools { get; set; } = new();

    public SchoolResponse? CurrentSchool { get; set; }


    public SelectList SchoolsList { get; set; }
    public string NewSchoolCode { get; set; } = string.Empty;
}
