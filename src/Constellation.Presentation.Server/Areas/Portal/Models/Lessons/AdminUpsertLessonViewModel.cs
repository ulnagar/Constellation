using Constellation.Application.DTOs;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class AdminUpsertLessonViewModel : BaseViewModel
    {
        public LessonDto Lesson { get; set; }

        public bool DoNotGenerateRolls { get; set; }
        public SelectList CourseList { get; set; }
    }
}