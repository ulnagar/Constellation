namespace Constellation.Presentation.Schools.Areas.Schools.Pages.Shared.Components.SchoolSelector;

using Application.Domains.Schools.Queries.GetSchoolsForContact;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

public sealed class SchoolSelectorModalViewModel
{
    public List<SchoolResponse> ValidSchools { get; set; } = new();

    public SchoolResponse? CurrentSchool { get; set; }


    public SelectList SchoolsList { get; set; }
    public string NewSchoolCode { get; set; }
}
