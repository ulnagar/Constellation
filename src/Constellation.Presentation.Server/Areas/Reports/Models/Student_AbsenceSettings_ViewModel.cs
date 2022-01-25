using Constellation.Core.Enums;
using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Reports.Models
{
    public class Student_AbsenceSettings_ViewModel : BaseViewModel
    {
        public Student_AbsenceSettings_ViewModel()
        {
            Students = new List<StudentItem>();
        }

        public ICollection<StudentItem> Students { get; set; }

        public class StudentItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string SchoolName { get; set; }
            public Grade Grade { get; set; }
            public bool IsEnabled { get; set; }
            public DateTime? EnabledFromDate { get; set; }
        }
    }
}