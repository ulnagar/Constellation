using Constellation.Application.Helpers;
using Constellation.Core.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Reports.Models
{
    public class Student_AttendanceQuerySelectionViewModel : BaseViewModel
    {
        [Required]
        [Display(Name = DisplayNameDefaults.Student)]
        public string StudentId { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name=DisplayNameDefaults.LookupDate)]
        public DateTime LookupDate { get; set; }
        public SelectList StudentList { get; set; }
        public ICollection<Student_AttendanceQuerySelection_ClassViewModel> ClassList { get; set; }
        public ICollection<TimetablePeriod> Periods { get; set; }

        public Student_AttendanceQuerySelectionViewModel()
        {
            ClassList = new List<Student_AttendanceQuerySelection_ClassViewModel>();
            Periods = new List<TimetablePeriod>();
            LookupDate = DateTime.Today;
        }
    }

    public class Student_AttendanceQuerySelection_ClassViewModel
    {
        public int PeriodId { get; set; }
        public string PeriodName { get; set; }
        public OfferingId ClassId { get; set; }
        public string ClassName { get; set; }
        public string RoomSco { get; set; }
    }
}