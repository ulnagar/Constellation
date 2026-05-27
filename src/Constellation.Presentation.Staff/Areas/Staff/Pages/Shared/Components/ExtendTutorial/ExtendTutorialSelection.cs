namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.ExtendTutorial;

using Application.Domains.LinkedSystems.Sentral.Queries.GetTermsAndWeeksForCurrentYear;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public sealed class ExtendTutorialSelection
{
    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly EndDate { get; set; }

    public List<SchoolCalendarWeek> ValidEndDates { get; set; } = new();
}
