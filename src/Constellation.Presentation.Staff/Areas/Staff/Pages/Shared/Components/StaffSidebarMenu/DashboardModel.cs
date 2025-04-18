namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.StaffSidebarMenu;

using Application.Timetables.GetStaffDailyTimetableData;
using Constellation.Core.Models.Offerings.Identifiers;
using Core.Models.Timetables.Enums;
using System.Collections.Generic;

public class DashboardModel
{
    public bool IsAdmin { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public int ExpiringTraining { get; set; } = 0;
    public Dictionary<string, OfferingId> Classes { get; set; } = new();

    public List<StaffDailyTimetableResponse> Periods { get; set; } = [];

    public PeriodWeek Week { get; set; }
    public PeriodDay Day { get; set; }
}