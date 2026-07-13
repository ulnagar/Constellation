namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Devices;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Archive.Queries.GetDevices;
using Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.Threading.Tasks;

[HasPermission(AuthPermission.Equipment_Assets_Edit_Value)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public IndexModel(
        ISender mediator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<IndexModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Devices_Devices;
    [ViewData] public string PageTitle => "Devices List";
    
    public List<DeviceSummaryResponse> Devices { get; set; }

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve device list by user {User}", _currentUserService.UserName);

        Result<List<DeviceSummaryResponse>> devices = await _mediator.Send(new GetDevicesQuery());

        if (devices.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(devices.Error);

            _logger
                .ForContext(nameof(Error), devices.Error, true)
                .Warning("Failed to retrieve device list by user {User}", _currentUserService.UserName);

            return;
        }

        Devices = devices.Value;
    }
}
