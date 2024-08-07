namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools;

using Application.Interfaces.Repositories;
using Constellation.Application.DTOs;
using Core.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

public class LocationsModel : BasePageModel
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public LocationsModel(
        ISchoolRepository schoolRepository,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _schoolRepository = schoolRepository;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<LocationsModel>()
            .ForContext(StaffLogDefaults.Application, StaffLogDefaults.StaffPortal);
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Partner_Schools_Schools;
    // PageTitle view data is excluded here intentionally

    public List<MapLayer> Layers { get; set; } = new();
    public string PageHeading { get; set; } = "Map of Schools";

    public bool Anonymous { get; set; }

    public void OnGet()
    {
        _logger
            .Information("Requested to retrieve map of Schools by anonymous user");

        Layers = _schoolRepository.GetForMapping(new List<string>()).ToList();
        Anonymous = true;
    }

    public async Task<IActionResult> OnPost(List<string> schoolCodes)
    {
        _logger
            .Information("Requested to retrieve map of Schools by user {User}", _currentUserService.UserName);

        Layers = _schoolRepository.GetForMapping(schoolCodes).ToList();

        return Page();
    }
}