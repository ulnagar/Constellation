namespace Constellation.Presentation.Staff.Areas.Staff.Pages.Shared.Components.AddDownloadToAssessment;

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public sealed class AddDownloadToAssessmentSelection
{
    [Required]
    public IFormFile UploadFile { get; set; }

    [Required]
    public string Name { get; set; }

    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly AvailableFrom { get; set; }
    
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateOnly AvailableTo { get; set; }
    
    public bool IsRestricted { get; set; }
}
