using Constellation.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Portal.Models.Lessons
{
    public class AdminUpsertLessonViewModel
    {
        public LessonDto Lesson { get; set; }

        public bool DoNotGenerateRolls { get; set; }
        public SelectList CourseList { get; set; }
    }
}