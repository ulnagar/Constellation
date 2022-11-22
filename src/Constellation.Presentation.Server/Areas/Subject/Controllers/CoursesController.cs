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

        public CoursesController(IUnitOfWork unitOfWork, ICourseOfferingService offeringService,
            IMediator mediator)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _offeringService = offeringService;
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return RedirectToAction("WithClasses");
        }

        public async Task<IActionResult> All()
        {
            var courses = await _unitOfWork.Courses.ForListAsync(course => true);

            var viewModel = await CreateViewModel<Course_ViewModel>();
            viewModel.Courses = courses.Select(Course_ViewModel.CourseDto.ConvertFromCourse).ToList();
            viewModel.FacultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithClasses()
        {
            var courses = await _unitOfWork.Courses.ForListAsync(course => course.Offerings.Any(offering => offering.StartDate <= DateTime.Now && offering.EndDate >= DateTime.Now));

            var viewModel = await CreateViewModel<Course_ViewModel>();
            viewModel.Courses = courses.Select(Course_ViewModel.CourseDto.ConvertFromCourse).ToList();
            viewModel.FacultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithoutClasses()
        {
            var courses = await _unitOfWork.Courses.ForListAsync(course => !course.Offerings.Any() || !course.Offerings.All(offering => offering.StartDate <= DateTime.Now && offering.EndDate >= DateTime.Now));

            var viewModel = await CreateViewModel<Course_ViewModel>();
            viewModel.Courses = courses.Select(Course_ViewModel.CourseDto.ConvertFromCourse).ToList();
            viewModel.FacultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

            return View("Index", viewModel);
        }

        public async Task<IActionResult> ForGrade(Grade id)
        {
            var courses = await _unitOfWork.Courses.ForListAsync(course => course.Grade == id);

            var viewModel = await CreateViewModel<Course_ViewModel>();
            viewModel.Courses = courses.Select(Course_ViewModel.CourseDto.ConvertFromCourse).ToList();
            viewModel.FacultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

            return View("Index", viewModel);
        }

        public async Task<IActionResult> ForFaculty(Guid facultyId)
        {
            var courses = await _unitOfWork.Courses.ForListAsync(course => course.FacultyId == facultyId);

            var viewModel = await CreateViewModel<Course_ViewModel>();
            viewModel.Courses = courses.Select(Course_ViewModel.CourseDto.ConvertFromCourse).ToList();
            viewModel.FacultyList = await _mediator.Send(new GetDictionaryOfFacultiesQuery());

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var course = await _unitOfWork.Courses.ForDetailDisplayAsync(id);

            if (course == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = await CreateViewModel<Course_DetailsViewModel>();
            viewModel.Course = Course_DetailsViewModel.CourseDto.ConvertFromCourse(course);
            viewModel.Course.FTECalculation = course.Offerings.Where(offering => offering.StartDate <= DateTime.Now && offering.EndDate >= DateTime.Now).SelectMany(offering => offering.Enrolments).Count(enrolment => !enrolment.IsDeleted) * course.FullTimeEquivalentValue;
            viewModel.Courses = course.Offerings.Select(Course_DetailsViewModel.OfferingDto.ConvertFromOffering).ToList();

            return View(viewModel);
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

            await _unitOfWork.CompleteAsync();

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