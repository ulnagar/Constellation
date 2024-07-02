namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Devices;

using Application.Common.PresentationModels;
using Application.Devices.GetDevices;
using Application.Models.Auth;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class IndexModel : BasePageModel
{
    private readonly ISender _mediator;

    public IndexModel(
        ISender mediator)
    {
        _mediator = mediator;
    }

    [ViewData] public string ActivePage => Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Devices_Devices;
    
    public List<DeviceSummaryResponse> Devices { get; set; }

    public async Task OnGet()
    {
        Result<List<DeviceSummaryResponse>> devices = await _mediator.Send(new GetDevicesQuery());

        if (devices.IsFailure)
        {
            ModalContent = new ErrorDisplay(devices.Error);

            return;
        }

        Devices = devices.Value;
    }
}
