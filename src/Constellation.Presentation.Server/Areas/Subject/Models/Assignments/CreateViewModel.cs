using Constellation.Application.Features.Subject.Assignments.Commands;
using Constellation.Application.Features.Subject.Assignments.Models;
using Constellation.Presentation.Server.BaseModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Constellation.Presentation.Server.Areas.Subject.Models.Assignments
{
    public class CreateViewModel : BaseViewModel
    {
        public CreateCanvasAssignmentCommand Command { get; set; } = new CreateCanvasAssignmentCommand();
     
        public ICollection<AssignmentFromCourseForDropdownSelection> Assignments { get; set; } = new List<AssignmentFromCourseForDropdownSelection>();
        public SelectList CoursesList { get; set; }
        public string CourseName { get; set; }
        public ICollection<SelectListItem> AssignmentsList { get; set; }
    }
}
