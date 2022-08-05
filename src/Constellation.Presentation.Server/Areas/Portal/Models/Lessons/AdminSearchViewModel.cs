using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class AdminSearchViewModel : BaseViewModel
    {
        public string SchoolCode { get; set; }
        public string StudentId { get; set; }

        public SelectList SchoolList { get; set; }
        public SelectList StudentList { get; set; }
    }
}