namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.CancelTutorial;

using System;
using System.ComponentModel.DataAnnotations;

public sealed class CancelTutorialSelection
{
    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly EndDate { get; set; }
}
