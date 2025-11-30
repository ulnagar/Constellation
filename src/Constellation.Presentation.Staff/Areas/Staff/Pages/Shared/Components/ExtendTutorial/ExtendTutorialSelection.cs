namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ExtendTutorial;

using Application.Domains.Attendance.Reports.Queries.GetValidAttendanceReportDates;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class ExtendTutorialSelection
{
    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly EndDate { get; set; }

    public List<ValidAttendenceReportDate> ValidEndDates { get; set; } = new();
}
