using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Areas.Subject.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Subject.Controllers
{
    [Area("Subject")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.User)]
    public class ClassesController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICourseOfferingService _offeringService;
        private readonly IAdobeConnectService _adobeConnectService;
        private readonly IOperationService _operationsService;
        private readonly IStudentService _studentService;

        public ClassesController(IUnitOfWork unitOfWork, ICourseOfferingService offeringService,
            IAdobeConnectService adobeConnectService, IOperationService operationsService,
            IStudentService studentService)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _offeringService = offeringService;
            _adobeConnectService = adobeConnectService;
            _operationsService = operationsService;
            _studentService = studentService;
        }

        // GET: Subject/Classes
        public IActionResult Index()
        {
            return RedirectToAction("Active");
        }

        public async Task<IActionResult> All()
        {
            var offerings = await _unitOfWork.CourseOfferings.ForListAsync(offering => true);

            var viewModel = await CreateViewModel<Classes_ViewModel>();
            viewModel.Offerings = offerings.Select(Classes_ViewModel.OfferingDto.ConvertFromOffering).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Active()
        {
            var offerings = await _unitOfWork.CourseOfferings.ForListAsync(offering => offering.Sessions.Any(session => !session.IsDeleted) && offering.EndDate >= DateTime.Now && offering.StartDate <= DateTime.Now);

            var viewModel = await CreateViewModel<Classes_ViewModel>();
            viewModel.Offerings = offerings.Select(Classes_ViewModel.OfferingDto.ConvertFromOffering).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Inactive()
        {
            var offerings = await _unitOfWork.CourseOfferings.ForListAsync(offering => offering.EndDate < DateTime.Now);

            var viewModel = await CreateViewModel<Classes_ViewModel>();
            viewModel.Offerings = offerings.Select(Classes_ViewModel.OfferingDto.ConvertFromOffering).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Upcoming()
        {
            var offerings = await _unitOfWork.CourseOfferings.ForListAsync(offering => offering.StartDate > DateTime.Now);

            var viewModel = await CreateViewModel<Classes_ViewModel>();
            viewModel.Offerings = offerings.Select(Classes_ViewModel.OfferingDto.ConvertFromOffering).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> FromGrade(Grade id)
        {
            var offerings = await _unitOfWork.CourseOfferings.ForListAsync(offering => offering.Course.Grade == id && offering.EndDate >= DateTime.Now);

            var viewModel = await CreateViewModel<Classes_ViewModel>();
            viewModel.Offerings = offerings.Select(Classes_ViewModel.OfferingDto.ConvertFromOffering).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> FromFaculty(Faculty id)
        {
            var offerings = await _unitOfWork.CourseOfferings.ForListAsync(offering => offering.Course.Faculty.HasFlag(id) && offering.EndDate >= DateTime.Now);

            var viewModel = await CreateViewModel<Classes_ViewModel>();
            viewModel.Offerings = offerings.Select(Classes_ViewModel.OfferingDto.ConvertFromOffering).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Details(int id)
        { 
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var offering = await _unitOfWork.CourseOfferings.ForDetailDisplayAsync(id);

            if (offering == null)
            {
                return RedirectToAction("Index");
            }

            var periods = await _unitOfWork.Periods.ForSelectionAsync();
            var staff = await _unitOfWork.Staff.ForSelectionAsync();
            var rooms = await _unitOfWork.AdobeConnectRooms.ForSelectionAsync();
            var students = await _unitOfWork.Students.ForSelectionListAsync();

            var periodSource = periods.Select(period => new { period.Id, period.Name, Group = period.GetPeriodGroup() }).ToList();

            var viewModel = await CreateViewModel<Classes_DetailsViewModel>();
            viewModel.Class = Classes_DetailsViewModel.OfferingDto.ConvertFromOffering(offering);
            viewModel.Students = offering.Enrolments.Where(enrol => !enrol.IsDeleted).Select(enrol => enrol.Student).Select(Classes_DetailsViewModel.StudentDto.ConvertFromStudent).ToList();
            viewModel.Sessions = offering.Sessions.Where(session => !session.IsDeleted).Select(Classes_DetailsViewModel.SessionDto.ConvertFromSession).ToList();
            viewModel.Lessons = offering.Lessons.Select(lesson => Classes_DetailsViewModel.LessonDto.ConvertFromLesson(lesson, offering)).ToList();
            viewModel.StudentList = new SelectList(students, "StudentId", "DisplayName", null, "CurrentGrade");
            viewModel.AddSessionDto = new Sessions_AssignmentViewModel
            {
                OfferingId = offering.Id,
                OfferingName = offering.Name,
                PeriodList = new SelectList(periodSource, "Id", "Name", null, "Group"),
                StaffList = new SelectList(staff.OrderBy(member => member.LastName), "StaffId", "DisplayName"),
                RoomList = new SelectList(rooms, "ScoId", "Name")
            };

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Update(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var offering = await _unitOfWork.CourseOfferings.ForEditAsync(id);

            var courseList = await _unitOfWork.Courses.ForSelectionAsync();

            var viewModel = await CreateViewModel<Classes_UpdateViewModel>();
            viewModel.Offering = new CourseOfferingDto
            {
                Id = offering.Id,
                Name = offering.Name,
                StartDate = offering.StartDate,
                EndDate = offering.EndDate,
                CourseId = offering.CourseId
            };
            viewModel.IsNew = false;
            viewModel.CourseList = new SelectList(courseList, "Id", "Name", viewModel.Offering.CourseId, "Faculty");

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Classes_UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);
                var courseList = await _unitOfWork.Courses.ForSelectionAsync();
                viewModel.CourseList = new SelectList(courseList, "Id", "Name", viewModel.Offering.CourseId, "Faculty");

                return View("Update", viewModel);
            }

            CourseOffering offering;

            if (viewModel.IsNew)
            {
                // TODO: Convert services code to async
                var result = await _offeringService.CreateOffering(viewModel.Offering);

                if (result.Success)
                    offering = result.Entity;
                else
                {
                    await UpdateViewModel(viewModel);
                    var courseList = await _unitOfWork.Courses.ForSelectionAsync();
                    viewModel.CourseList = new SelectList(courseList, "Id", "Name", viewModel.Offering.CourseId, "Faculty");

                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error);

                    return View("Update", viewModel);
                }
            }
            else
            {
                var result = await _offeringService.UpdateOffering(viewModel.Offering);

                if (result.Success)
                    offering = result.Entity;
                else
                {
                    await UpdateViewModel(viewModel);
                    var courseList = await _unitOfWork.Courses.ForSelectionAsync();
                    viewModel.CourseList = new SelectList(courseList, "Id", "Name", viewModel.Offering.CourseId, "Faculty");

                    return View("Update", viewModel);
                }
            }

            await _unitOfWork.CompleteAsync();

            if (viewModel.CreateRoom)
            {
                await _adobeConnectService.CreateRoom(offering);

                await _operationsService.CreateClassroomMSTeam(offering.Id, DateTime.Now);
            }

            var lessons = await _unitOfWork.Lessons.GetAllForCourse(offering.CourseId);

            foreach (var lesson in lessons.Where(lesson => lesson.DueDate > DateTime.Today))
            {
                // Add offering to the lesson
                lesson.Offerings.Add(offering);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Create()
        {
            var viewModel = await CreateViewModel<Classes_UpdateViewModel>();
            viewModel.IsNew = true;
            var courses = await _unitOfWork.Courses.ForSelectionAsync();
            var courseList = courses.Select(course => new { course.Id, Name = $"Year {(int)course.Grade:D2} {course.Name}", Grouping = course.Faculty.ToString() });
            viewModel.CourseList = new SelectList(courseList, "Id", "Name", null, "Grouping");

            return View("Update", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Enrol(string id, int classId)
        {
            var student = await _unitOfWork.Students.ForBulkUnenrolAsync(id);

            if (student == null)
            {
                return RedirectToAction("Index");
            }

            if (!student.Enrolments.Any(e => e.OfferingId == classId && !e.IsDeleted))
            {
                await _studentService.EnrolStudentInClass(student.StudentId, classId);
                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction("Details", new { area = "Subject", id = classId });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Unenrol(string id, int classId, string returnPage)
        {
            var student = await _unitOfWork.Students.ForBulkUnenrolAsync(id);

            foreach (var enrolment in student.Enrolments.Where(e => e.OfferingId == classId && !e.IsDeleted))
            {
                await _studentService.UnenrolStudentFromClass(student.StudentId, classId);
            }

            await _unitOfWork.CompleteAsync();

            if (returnPage == "student")
            {
                return RedirectToAction("Details", "Students", new { area = "Partner", id });
            }
            else if (returnPage == "class")
            {
                return RedirectToAction("Details", "Classes", new { area = "Subject", id = classId });
            }

            return RedirectToAction("Details", new { id });
        }
    }
}