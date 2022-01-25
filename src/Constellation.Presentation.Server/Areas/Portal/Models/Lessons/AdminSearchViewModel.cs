using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class AdminSearchViewModel
    {
        public string SchoolCode { get; set; }
        public string StudentId { get; set; }

        public SelectList SchoolList { get; set; }
        public SelectList StudentList { get; set; }
    }
}