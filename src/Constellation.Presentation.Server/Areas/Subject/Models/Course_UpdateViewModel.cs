using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Subject.Models
{
    public class Course_UpdateViewModel : BaseViewModel
    {
        public CourseDto Course { get; set; }
        public bool IsNew { get; set; }
        public SelectList StaffList { get; set; }
        public SelectList FacultyList { get; set; }
    }
}