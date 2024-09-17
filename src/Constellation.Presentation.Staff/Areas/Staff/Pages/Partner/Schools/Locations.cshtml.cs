namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Partner.Schools;

using Application.Common.PresentationModels;
using Application.Interfaces.Repositories;
using Application.Schools.GetSchoolLocationsAsMapLayers;
using Constellation.Application.DTOs;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Models;
using Serilog;

public class LocationsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public LocationsModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
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

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve map of Schools by anonymous user");

        Result<List<MapLayer>> layerRequest = await _mediator.Send(new GetSchoolLocationsAsMapLayersQuery(new()));

        if (layerRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), layerRequest.Error, true)
                .Warning("Failed to retrieve map of Schools by anonymous user");

            ModalContent = new ErrorDisplay(layerRequest.Error);
        }

        Layers = layerRequest.Value;
        Anonymous = true;
    }

    public async Task<IActionResult> OnPost(List<string> schoolCodes)
    {
        _logger
            .Information("Requested to retrieve map of Schools by user {User}", _currentUserService.UserName);

        Result<List<MapLayer>> layerRequest = await _mediator.Send(new GetSchoolLocationsAsMapLayersQuery(schoolCodes));

        if (layerRequest.IsFailure)
        {
            _logger
                .ForContext(nameof(Error), layerRequest.Error, true)
                .Warning("Failed to retrieve map of Schools by user {User}", _currentUserService.UserName);

            ModalContent = new ErrorDisplay(layerRequest.Error);
        }

        Layers = layerRequest.Value;

        return Page();
    }
}