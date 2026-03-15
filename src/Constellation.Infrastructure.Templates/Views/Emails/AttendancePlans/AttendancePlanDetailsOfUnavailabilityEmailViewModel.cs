namespace Constellation.Infrastructure.Templates.Views.Emails.AttendancePlans;

using Constellation.Infrastructure.Templates.Views.Shared;
using Core.Models.Timetables.Enums;
using System;
using System.Collections.Generic;

public sealed class AttendancePlanDetailsOfUnavailabilityEmailViewModel : EmailLayoutBaseViewModel
{
    private const string _viewLocation = "/Views/Emails/AttendancePlans/AttendancePlanDetailsOfUnavailabilityEmail.cshtml";
    public override string ViewLocation => _viewLocation;

    public required string Student { get; set; }
    public required string School { get; set; }
    public required string Grade { get; set; }

    public List<Unavailability> Unavailabilities { get; set; } = [];

    public sealed class Unavailability
    {
        public required PeriodWeek Week { get; set; }
        public required PeriodDay Day { get; set; }
        public required TimeOnly Start { get; set; }
        public required TimeOnly End { get; set; }
    }
}
