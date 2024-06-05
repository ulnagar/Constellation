namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools;

using Application.Interfaces.Repositories;
using Constellation.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

public class LocationsModel : BasePageModel
{
    private readonly ISchoolRepository _schoolRepository;

    public LocationsModel(
        ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Schools;

    public List<MapLayer> Layers { get; set; } = new();
    public string PageHeading { get; set; } = "Map of Schools";

    public bool Anonymous { get; set; }

    public void OnGet()
    {
        Layers = _schoolRepository.GetForMapping(new List<string>()).ToList();
        Anonymous = true;
    }

    public async Task<IActionResult> OnPost(List<string> schoolCodes)
    {
        Layers = _schoolRepository.GetForMapping(schoolCodes).ToList();

        return Page();
    }
}