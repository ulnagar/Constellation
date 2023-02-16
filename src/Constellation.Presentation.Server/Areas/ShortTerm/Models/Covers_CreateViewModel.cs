namespace Constellation.Presentation.Server.Areas.ShortTerm.Models;

using Constellation.Application.Helpers;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class Covers_CreateViewModel : BaseViewModel
{
    [Display(Name = DisplayNameDefaults.TeacherName)]
    public string UserId { get; set; }
    [Display(Name = DisplayNameDefaults.ClassName)]
    public int? ClassId { get; set; }
    public IEnumerable<int> SelectedClasses { get; set; }
    public string TeacherId { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = DisplayNameDefaults.DateStart)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime StartDate { get; set; }

    [Required]
    [Display(Name = DisplayNameDefaults.DateEnd)]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime EndDate { get; set; }

    public SelectList UserList { get; set; }
    public SelectList ClassList { get; set; }
    public SelectList TeacherList { get; set; }
    public MultiSelectList MultiClassList { get; set; }
}