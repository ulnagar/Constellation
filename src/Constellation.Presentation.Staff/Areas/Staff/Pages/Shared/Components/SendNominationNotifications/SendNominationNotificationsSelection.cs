namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.SendNominationNotifications;

using System;
using System.ComponentModel.DataAnnotations;

public sealed class SendNominationNotificationsSelection
{
    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly DeliveryDate { get; set; }
}
