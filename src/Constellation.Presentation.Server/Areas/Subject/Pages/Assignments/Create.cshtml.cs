using Constellation.Application.Common.CQRS.Subject.Assignments.Commands;
using Constellation.Application.Common.CQRS.Subject.Courses.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Presentation.Server.BaseModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Subject.Pages.Assignments
{
    public class CreateModel : BasePageModel
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public CreateModel(IUnitOfWork unitOfWork, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        [BindProperty]
        public CreateCanvasAssignmentCommand Command { get; set; } = new CreateCanvasAssignmentCommand();
        public SelectList CoursesList { get; set; }
        [BindProperty]
        public string SelectedCourseId { get; set; }

        public async void OnGet()
        {
            await GetClasses(_unitOfWork);

            var courses = await _mediator.Send(new GetCoursesForDropdownSelectionQuery());
            CoursesList = new SelectList(courses, nameof(CourseForDropdownSelection.Id), nameof(CourseForDropdownSelection.DisplayName), null, nameof(CourseForDropdownSelection.Faculty));
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await _mediator.Send(Command);

            return RedirectToPage("Index");
        }
    }
}
