namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.TransferStudent;

using Core.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public sealed class TransferStudentSelection
{
    public Grade Grade { get; set; }
    public string SchoolCode { get; set; } = string.Empty;
    [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly StartDate { get; set; }

    public required SelectList SchoolList { get; set; }
}