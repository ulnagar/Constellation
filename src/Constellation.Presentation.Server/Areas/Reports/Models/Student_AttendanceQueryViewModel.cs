using Constellation.Presentation.Server.BaseModels;
using System;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Reports.Models
{
    public class Student_AttendanceQueryViewModel : BaseViewModel
    {
        public ICollection<Student_AttendanceQuery_UserListViewModel> Users { get; set; }

        public Student_AttendanceQueryViewModel()
        {
            Users = new List<Student_AttendanceQuery_UserListViewModel>();
        }
    }

    public class Student_AttendanceQuery_UserListViewModel
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public DateTime? LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public bool Highlighted { get; set; }
    }
}