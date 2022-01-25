using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Subject.Models
{
    public class Classes_UpdateViewModel : BaseViewModel
    {
        public Classes_UpdateViewModel()
        {
            Offering = new CourseOfferingDto();
        }

        public CourseOfferingDto Offering { get; set; }
        public bool IsNew { get; set; }
        public SelectList CourseList { get; set; }
        public bool CreateRoom { get; set; }
    }
}