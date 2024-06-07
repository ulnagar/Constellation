namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Equipment.Devices;

using Application.Devices.GetDeviceDetails;
using Application.Models.Auth;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Threading.Tasks;

[Authorize(Policy = AuthPolicies.CanManageAssets)]
public class DetailsModel : BasePageModel
{
    private readonly ISender _mediator;
    private readonly LinkGenerator _linkGenerator;

    public DetailsModel(
        ISender mediator,
        LinkGenerator linkGenerator)
    {
        _mediator = mediator;
        _linkGenerator = linkGenerator;
    }

    [ViewData] public string ActivePage => Presentation.Staff.Pages.Shared.Components.StaffSidebarMenu.ActivePage.Equipment_Devices_Devices;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; }

    public DeviceDetailsResponse Device { get; set; }

    public async Task OnGet()
    {
        Result<DeviceDetailsResponse> device = await _mediator.Send(new GetDeviceDetailsQuery(Id));

        if (device.IsFailure)
        {
            Error = new()
            {
                Error = device.Error,
                RedirectPath = _linkGenerator.GetPathByPage("/Equipment/Devices/Index", values: new { area = "Staff"})
            };

            return;
        }

        Device = device.Value;
    }
}
