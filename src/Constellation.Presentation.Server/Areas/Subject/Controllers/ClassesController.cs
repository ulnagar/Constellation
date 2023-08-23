using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Features.Faculties.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions;
using Constellation.Core.Enums;
using Constellation.Core.Models.Subjects;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.Areas.Subject.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Subject.Controllers
{
    [Area("Subject")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class ClassesController : BaseController
    {
        private readonly IOfferingRepository _offeringRepository;
        private readonly ILessonRepository _lessonRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICourseOfferingService _offeringService;
        private readonly IAdobeConnectService _adobeConnectService;
        private readonly IOperationService _operationsService;
        private readonly IStudentService _studentService;
        private readonly IAppDbContext _context;
        private readonly IMediator _mediator;

        public ClassesController(
            IOfferingRepository offeringRepository,
            ILessonRepository lessonRepository,
            IUnitOfWork unitOfWork, 
            ICourseOfferingService offeringService,
            IAdobeConnectService adobeConnectService, 
            IOperationService operationsService,
            IStudentService studentService, 
            IAppDbContext context, 
            IMediator mediator)
            : base(unitOfWork)
        {
            _offeringRepository = offeringRepository;
            _lessonRepository = lessonRepository;
            _unitOfWork = unitOfWork;
            _offeringService = offeringService;
            _adobeConnectService = adobeConnectService;
            _operationsService = operationsService;
            _studentService = studentService;
            _context = context;
            _mediator = mediator;
        }

        public async Task<IActionResult> Details(Guid id)
        { 
            if (id == new Guid())
            {
                return RedirectToAction("Index");
            }

            OfferingId offeringId = OfferingId.FromValue(id);

            var offering = await _unitOfWork.CourseOfferings.ForDetailDisplayAsync(offeringId);

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
            viewModel.StudentList = new SelectList(students, "StudentId", "DisplayName", null, "CurrentGrade");
            viewModel.AddSessionDto = new Sessions_AssignmentViewModel
            {
                OfferingId = offering.Id,
                OfferingName = offering.Name,
                PeriodList = new SelectList(periodSource, "Id", "Name", null, "Group"),
                StaffList = new SelectList(staff.OrderBy(member => member.LastName), "StaffId", "DisplayName"),
                RoomList = new SelectList(rooms, "ScoId", "Name")
            };

            var lessons = await _lessonRepository.GetAllForOffering(offering.Id);

            foreach (var lesson in lessons)
            {
                var entry = new Classes_DetailsViewModel.LessonDto()
                {
                    Id = lesson.Id,
                    Name = lesson.Name,
                    DueDate = lesson.DueDate
                };

                foreach (var record in lesson.Rolls)
                {
                    var school = await _unitOfWork.Schools.GetById(record.SchoolCode);

                    foreach (var student in record.Attendance)
                    {
                        var enrolledStudent = viewModel.Students.Where(enrolledStudent => enrolledStudent.StudentId == student.StudentId).FirstOrDefault();

                        if (enrolledStudent is null)
                        {
                            continue;
                        }

                        entry.Students.Add(new()
                        {
                            SchoolName = school.Name,
                            Name = enrolledStudent.Name,
                            Status = record.Status,
                            Comment = record.Comment,
                            WasPresent = student.Present
                        });
                    }
                }

                if (entry.Students.Any())
                    viewModel.Lessons.Add(entry);
            }
            
            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Update(Guid id)
        {
            if (id == new Guid())
            {
                return RedirectToAction("Index");
            }

            OfferingId offeringId = OfferingId.FromValue(id);

            var offering = await _unitOfWork.CourseOfferings.ForEditAsync(offeringId);

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

            Offering offering;

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
                offering = await _unitOfWork.CourseOfferings.ForCoverCreationAsync(offering.Id);

                await _adobeConnectService.CreateRoom(offering);

                //await _operationsService.CreateClassroomMSTeam(offering.Id, DateTime.Now);
            }

            var lessons = await _lessonRepository.GetAllForCourse(offering.CourseId);

            foreach (var lesson in lessons.Where(lesson => lesson.DueDate > DateOnly.FromDateTime(DateTime.Today)))
            {
                // Add offering to the lesson
                lesson.AddOffering(offering.Id);
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
            var courseList = courses.Select(course => new { course.Id, Name = $"Year {(int)course.Grade:D2} {course.Name}", Grouping = course.Faculty.Name });
            viewModel.CourseList = new SelectList(courseList, "Id", "Name", null, "Grouping");

            return View("Update", viewModel);
        }

        [Authorize]
        public async Task<IActionResult> Map(Guid id)
        {
            var vm = await CreateViewModel<School_MapViewModel>();

            OfferingId offeringId = OfferingId.FromValue(id);

            var offering = await _unitOfWork.CourseOfferings.ForEditAsync(offeringId);

            var schoolCodes = await _context.Enrolments
                .Where(enrolment => enrolment.OfferingId == offeringId && !enrolment.IsDeleted)
                .Select(enrolment => enrolment.Student.SchoolCode)
                .Distinct()
                .ToListAsync();

            var teacherCodes = await _context.Sessions
                .Where(session => session.OfferingId == offeringId && !session.IsDeleted)
                .Select(session => session.Teacher.SchoolCode)
                .Distinct()
                .ToListAsync();

            schoolCodes.AddRange(teacherCodes);
            schoolCodes = schoolCodes.Distinct().ToList();

            vm.Layers = _unitOfWork.Schools.GetForMapping(schoolCodes);
            vm.PageHeading = $"Map of {offering.Name}";

            return View("Map", vm);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Enrol(string id, Guid classId)
        {
            var student = await _unitOfWork.Students.ForBulkUnenrolAsync(id);

            if (student == null)
            {
                return RedirectToAction("Index");
            }

            OfferingId offeringId = OfferingId.FromValue(classId);

            if (!student.Enrolments.Any(e => e.OfferingId == offeringId && !e.IsDeleted))
            {
                await _studentService.EnrolStudentInClass(student.StudentId, offeringId);
                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction("Details", new { area = "Subject", id = classId });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Unenrol(string id, Guid classId, string returnPage)
        {
            var student = await _unitOfWork.Students.ForBulkUnenrolAsync(id);

            OfferingId offeringId = OfferingId.FromValue(classId);

            foreach (var enrolment in student.Enrolments.Where(e => e.OfferingId == offeringId && !e.IsDeleted))
            {
                await _studentService.UnenrolStudentFromClass(student.StudentId, offeringId);
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