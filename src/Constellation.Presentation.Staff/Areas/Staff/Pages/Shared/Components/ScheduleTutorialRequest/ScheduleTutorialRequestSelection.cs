namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ScheduleTutorialRequest;

using Constellation.Application.Domains.StaffMembers.Models;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Timetables.Identifiers;
using System.Collections.Generic;

public sealed class ScheduleTutorialRequestSelection
{
    public List<PeriodDecision> Periods = [];
    public string Comment { get; set; }

    public List<StaffSelectionListResponse> StaffMembers = [];

    public sealed class PeriodDecision
    {
        public PeriodDecision(
            PeriodId periodId,
            string periodName)
        {
            PeriodId = periodId;
            PeriodName = periodName;
            StaffId = StaffId.Empty;
        }

        public PeriodId PeriodId { get; init; }
        public string PeriodName { get; init; }
        public StaffId StaffId { get; set; }
    }
}
