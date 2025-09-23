namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ScheduleTutorialRequest;

using Application.Domains.Attendance.Reports.Queries.GetValidAttendanceReportDates;
using Constellation.Application.Domains.StaffMembers.Models;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Identifiers;
using Core.Models.Tutorials.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

public sealed class ScheduleTutorialRequestSelection
{
    public List<PeriodDecision> Periods { get; set; } = [];

    public string Comment { get; set; }
    public DateTime? StartWeek { get; set; }

    public TutorialName Name { get; set; }

    public List<StaffSelectionListResponse> StaffMembers = [];
    public List<ValidAttendenceReportDate> ValidStartDates = [];

    public sealed class PeriodDecision
    {
        public PeriodDecision() { }

        public PeriodDecision(
            PeriodId periodId,
            string periodName)
        {
            PeriodId = periodId;
            PeriodName = periodName;
            StaffId = StaffId.Empty;
        }

        public PeriodId PeriodId { get; set; }
        public string PeriodName { get; set; }
        public StaffId StaffId { get; set; }
    }
}
