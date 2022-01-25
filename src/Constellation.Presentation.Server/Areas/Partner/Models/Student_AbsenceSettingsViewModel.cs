using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.ComponentModel.DataAnnotations;

namespace Constellation.Presentation.Server.Areas.Partner.Models
{
    public class Student_AbsenceSettingsViewModel : BaseViewModel
    {
        public SelectList SchoolList { get; set; }
        public SelectList StudentList { get; set; }
        public SelectList GradeList { get; set; }

        public string StudentId { get; set; }
        public string SchoolCode { get; set; }
        public string Grade { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        public Student_AbsenceSettingsViewModel()
        {
            StartDate = DateTime.Today.AddDays(1);
        }
    }
}