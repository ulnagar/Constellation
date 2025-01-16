namespace Constellation.Infrastructure.Templates.Views.Emails.AttendancePlans;

using Constellation.Infrastructure.Templates.Views.Shared;
using Core.Models.Timetables.Enums;
using System;
using System.Collections.Generic;

public sealed class AttendancePlanDetailsOfUnavailabilityEmailViewModel : EmailLayoutBaseViewModel
{
    public const string ViewLocation = "/Views/Emails/AttendancePlans/AttendancePlanDetailsOfUnavailabilityEmail.cshtml";

    public string Student { get; set; }
    public string School { get; set; }
    public string Grade { get; set; }

    public List<Unavailability> Unavailabilities { get; set; } = new();

    public sealed class Unavailability
    {
        public PeriodWeek Week { get; set; }
        public PeriodDay Day { get; set; }
        public TimeOnly Start { get; set; }
        public TimeOnly End { get; set; }
    }
}
