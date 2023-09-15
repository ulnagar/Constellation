using Constellation.Application.DTOs;
using Constellation.Application.Features.Faculties.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Presentation.Server.Areas.Subject.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Subject.Controllers
{
    [Area("Subject")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class CoursesController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICourseOfferingService _offeringService;
        private readonly IMediator _mediator;

        public CoursesController(
            IUnitOfWork unitOfWork, 
            ICourseOfferingService offeringService,
            IMediator mediator)
            : base(mediator)
        {
            _unitOfWork = unitOfWork;
            _offeringService = offeringService;
            _mediator = mediator;
        }


        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Update(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var course = await _unitOfWork.Courses.ForEditAsync(id);

            if (course == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = await CreateViewModel<Course_UpdateViewModel>();
            viewModel.Course = new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Grade = course.Grade,
                FacultyId = course.FacultyId,
                Faculty = course.Faculty,
                FullTimeEquivalentValue = course.FullTimeEquivalentValue
            };

            var faculties = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

            viewModel.FacultyList = new SelectList(faculties, "Key", "Value");
            viewModel.StaffList = new SelectList(await _unitOfWork.Staff.ForSelectionAsync(), "StaffId", "DisplayName");
            viewModel.IsNew = false;

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Course_UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);
                viewModel.StaffList = new SelectList(await _unitOfWork.Staff.ForSelectionAsync(), "StaffId", "DisplayName");
                return View("Update", viewModel);
            }

            if (viewModel.IsNew)
            {
                await _offeringService.CreateCourse(viewModel.Course);
            }
            else
            {
                await _offeringService.UpdateCourse(viewModel.Course);
            }

            return RedirectToAction("Index");
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Create()
        {
            var viewModel = await CreateViewModel<Course_UpdateViewModel>();
            viewModel.StaffList = new SelectList(await _unitOfWork.Staff.ForSelectionAsync(), "StaffId", "DisplayName");
            viewModel.IsNew = true;

            return View("Update", viewModel);
        }
    }
}