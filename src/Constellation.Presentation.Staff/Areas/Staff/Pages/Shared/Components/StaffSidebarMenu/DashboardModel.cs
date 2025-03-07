namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StaffSidebarMenu;

using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public class DashboardModel
{
    public bool IsAdmin { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public int ExpiringTraining { get; set; } = 0;
    public Dictionary<string, OfferingId> Classes { get; set; } = new();
}