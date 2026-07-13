namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Devices;

using Application.Common.PresentationModels;
using Application.Domains.AssetManagement.Archive.Queries.GetDeviceDetails;
using Application.Models.Auth;
using Constellation.Presentation.Shared.Helpers.Attributes;
using Core.Abstractions.Services;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Presentation.Shared.Extensions;
using Presentation.Shared.Helpers.Logging;
using Serilog;
using System.Threading.Tasks;

[HasPermission(AuthPermission.Equipment_Assets_Edit_Value)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger _logger;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator,
        ICurrentUserService currentUserService,
        ILogger logger)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
        _currentUserService = currentUserService;
        _logger = logger
            .ForContext<DetailsModel>()
            .ForStaffPortal();
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Devices_Devices;
    [ViewData] public string PageTitle => string.IsNullOrWhiteSpace(Id) ? "Device Details" : $"Device Details - {Id}";


    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public DeviceDetailsResponse Device { get; set; }

    public async Task OnGet()
    {
        _logger
            .Information("Requested to retrieve details of device with Id {Id} by user {User}", Id, _currentUserService.UserName);

        Result<DeviceDetailsResponse> device = await _mediator.Send(new GetDeviceDetailsQuery(Id));

        if (device.IsFailure)
        {
            ModalContent = ErrorDisplay.Create(
                device.Error,
                _linkGenerator.GetPathByPage("/Equipment/Devices/Index", values: new { area = "Staff"}));

            _logger
                .ForContext(nameof(Error), device.Error, true)
                .Warning("Failed to retrieve details of device with Id {Id} by user {User}", Id, _currentUserService.UserName);

            return;
        }

        Device = device.Value;
    }
}
