namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.SendNominationNotifications;

using Core.Abstractions.Clock;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

public sealed class SendNominationNotificationsViewComponent : ViewComponent
{
    private readonly IDateTimeProvider _dateTime;

    public SendNominationNotificationsViewComponent(
        IDateTimeProvider dateTime)
    {
        _dateTime = dateTime;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        SendNominationNotificationsSelection viewModel = new()
        {
            DeliveryDate = _dateTime.Today
        };

        return View(viewModel);
    }
}
