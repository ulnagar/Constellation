using Constellation.Application.Features.Gateways.CanvasGateway.Notifications;
using Constellation.Application.Features.Subject.Assignments.Queries;
using Constellation.Application.Features.Subject.Courses.Models;
using Constellation.Application.Features.Subject.Courses.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.Areas.Subject.Models.Assignments;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Subject.Controllers
{
    [Area("Subject")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.User)]
    public class AssignmentsController : BaseController
    {
        private readonly IMediator _mediator;

        public AssignmentsController(IUnitOfWork unitOfWork, IMediator mediator)
            : base(unitOfWork)
        {
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await CreateViewModel<IndexViewModel>();
            viewModel.Assignments = await _mediator.Send(new GetAssignmentsQuery());

            return View(viewModel);
        }

        [Route("Create/Step1")]
        public async Task<IActionResult> Create_Step1()
        {
            var viewModel = await CreateViewModel<CreateViewModel>();
            var courses = await _mediator.Send(new GetCoursesForDropdownSelectionQuery());
            viewModel.CoursesList = new SelectList(courses, nameof(CourseForDropdownSelection.Id), nameof(CourseForDropdownSelection.DisplayName), null, nameof(CourseForDropdownSelection.Faculty));

            return View(viewModel);
        }

        [HttpPost]
        [Route("Create/Step2")]
        public async Task<IActionResult> Create_Step2(CreateViewModel viewModel)
        {
            await UpdateViewModel(viewModel);

            var courses = await _mediator.Send(new GetCoursesForDropdownSelectionQuery());
            viewModel.CourseName = courses.FirstOrDefault(course => course.Id == viewModel.Command.CourseId)?.DisplayName;

            var canvasAssignments = await _mediator.Send(new GetAssignmentsFromCourseForDropdownSelectionQuery { CourseId = viewModel.Command.CourseId });
            if (!canvasAssignments.IsValidResponse)
            {
                viewModel.CoursesList = new SelectList(courses, nameof(CourseForDropdownSelection.Id), nameof(CourseForDropdownSelection.DisplayName), null, nameof(CourseForDropdownSelection.Faculty));

                foreach (var error in canvasAssignments.Errors)
                    ModelState.AddModelError("", error);

                return View("Create_Step1", viewModel);
            }

            viewModel.Assignments = canvasAssignments.Result;
            viewModel.AssignmentsList = viewModel.Assignments.Select(a => new SelectListItem { Text = a.Name, Value = a.CanvasId.ToString(), Disabled = a.ExistsInDatabase }).ToList();

            return View(viewModel);
        }

        [HttpPost]
        [Route("Create/Step3")]
        public async Task<IActionResult> Create_Step3(CreateViewModel viewModel)
        {
            await UpdateViewModel(viewModel);

            var courses = await _mediator.Send(new GetCoursesForDropdownSelectionQuery());
            viewModel.CourseName = courses.FirstOrDefault(course => course.Id == viewModel.Command.CourseId)?.DisplayName;

            var canvasAssignments = await _mediator.Send(new GetAssignmentsFromCourseForDropdownSelectionQuery { CourseId = viewModel.Command.CourseId });
            viewModel.Assignments = canvasAssignments.Result;

            var selectedAssignment = viewModel.Assignments.FirstOrDefault(assignment => assignment.CanvasId == viewModel.Command.CanvasId);

            viewModel.Command.Name = selectedAssignment.Name;
            viewModel.Command.DueDate = selectedAssignment.DueDate;
            viewModel.Command.LockDate = selectedAssignment.LockDate;
            viewModel.Command.UnlockDate = selectedAssignment.UnlockDate;
            viewModel.Command.AllowedAttempts = selectedAssignment.AllowedAttempts;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create_Step4(CreateViewModel viewModel)
        {
            var result = await _mediator.Send(viewModel.Command);

            if (!result.IsValidResponse)
            {
                await UpdateViewModel(viewModel);

                var courses = await _mediator.Send(new GetCoursesForDropdownSelectionQuery());
                viewModel.CourseName = courses.FirstOrDefault(course => course.Id == viewModel.Command.CourseId)?.DisplayName;

                var canvasAssignments = await _mediator.Send(new GetAssignmentsFromCourseForDropdownSelectionQuery { CourseId = viewModel.Command.CourseId });
                viewModel.Assignments = canvasAssignments.Result;

                var selectedAssignment = viewModel.Assignments.FirstOrDefault(assignment => assignment.CanvasId == viewModel.Command.CanvasId);

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }

                return View("Create_Step3", viewModel);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var viewModel = await CreateViewModel<DetailsViewModel>();
            viewModel.Assignment = await _mediator.Send(new GetAssignmentQuery { Id = id });
            viewModel.Submissions = await _mediator.Send(new GetAssignmentSubmissionsQuery { Id = id });

            return View(viewModel);
        }

        public async Task<IActionResult> ResubmitToCanvas(Guid id)
        {
            await _mediator.Publish(new CanvasAssignmentSubmissionUploadedNotification { Id = id });

            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
