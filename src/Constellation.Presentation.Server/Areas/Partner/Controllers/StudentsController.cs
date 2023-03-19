using Constellation.Application.DTOs;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class StudentsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentService _studentService;
        private readonly IOperationService _operationService;
        private readonly IMediator _mediator;

        public StudentsController(IUnitOfWork unitOfWork, IStudentService studentService,
            IOperationService operationService, IMediator mediator)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _studentService = studentService;
            _operationService = operationService;
            _mediator = mediator;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Active");
        }

        public async Task<IActionResult> All()
        {
            var students = await _unitOfWork.Students.ForListAsync(student => true);

            var viewModel = await CreateViewModel<Student_ViewModel>();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Active()
        {
            var students = await _unitOfWork.Students.ForListAsync(student => !student.IsDeleted);

            var viewModel = await CreateViewModel<Student_ViewModel>();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Inactive()
        {
            var students = await _unitOfWork.Students.ForListAsync(student => student.IsDeleted);

            var viewModel = await CreateViewModel<Student_ViewModel>();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithoutACDetails()
        {
            var students = await _unitOfWork.Students.ForListAsync(student => string.IsNullOrWhiteSpace(student.AdobeConnectPrincipalId));

            var viewModel = await CreateViewModel<Student_ViewModel>();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithoutDevice()
        {
            var students = await _unitOfWork.Students.ForListAsync(student => student.Devices.Any(device => !device.IsDeleted));

            var viewModel = await CreateViewModel<Student_ViewModel>();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> ClassAudit()
        {
            var students = await _unitOfWork.Students.AllActiveForClassAuditAsync();

            var viewModel = await CreateViewModel<Student_ViewModel>();
            viewModel.Students = students.Select(Student_ViewModel.StudentDto.ConvertFromStudent).ToList();

            return View("ClassAudit", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Update(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var student = await _unitOfWork.Students.ForEditAsync(id);

            if (student == null)
            {
                return RedirectToAction("Index");
            }

            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var genders = new List<SelectListItem>()
            {
                new SelectListItem() { Text ="Male", Value = "M" },
                new SelectListItem() { Text = "Female", Value = "F" }
            };

            var viewModel = await CreateViewModel<Student_UpdateViewModel>();
            viewModel.Student = new StudentDto
            {
                StudentId = student.StudentId,
                FirstName = student.FirstName,
                LastName = student.LastName,
                PortalUsername = student.PortalUsername,
                AdobeConnectPrincipalId = student.AdobeConnectPrincipalId,
                SentralStudentId = student.SentralStudentId,
                EnrolledGrade = student.EnrolledGrade,
                CurrentGrade = student.CurrentGrade,
                Gender = student.Gender,
                SchoolCode = student.SchoolCode
            };
            viewModel.IsNew = false;
            viewModel.SchoolList = new SelectList(schools, "Code", "Name", student.SchoolCode);
            viewModel.GenderList = new SelectList(genders, "Value", "Text", student.Gender);

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Student_UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var schools = await _unitOfWork.Schools.ForSelectionAsync();
                var genders = new List<SelectListItem>()
                {
                    new SelectListItem() { Text ="Male", Value = "M" },
                    new SelectListItem() { Text = "Female", Value = "F" }
                };
                await UpdateViewModel(viewModel);
                viewModel.SchoolList = new SelectList(schools, "Code", "Name", viewModel.Student.SchoolCode);
                viewModel.GenderList = new SelectList(genders, "Value", "Text", viewModel.Student.Gender);

                return View("Update", viewModel);
            }

            if (viewModel.IsNew)
            {
                var result = await _studentService.CreateStudent(viewModel.Student);
                await _unitOfWork.CompleteAsync();
                await _operationService.CreateStudentEnrolmentMSTeamAccess(viewModel.Student.StudentId);
                await _operationService.CreateCanvasUserFromStudent(result.Entity);
            }
            else
            {
                await _studentService.UpdateStudent(viewModel.Student.StudentId, viewModel.Student);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Create()
        {
            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var genders = new List<SelectListItem>()
            {
                new SelectListItem() { Text ="Male", Value = "M" },
                new SelectListItem() { Text = "Female", Value = "F" }
            };

            var viewModel = await CreateViewModel<Student_UpdateViewModel>();
            viewModel.IsNew = true;
            viewModel.SchoolList = new SelectList(schools, "Code", "Name");
            viewModel.GenderList = new SelectList(genders, "Value", "Text");

            return View("Update", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> UnenrolAll(string id)
        {
            var student = await _unitOfWork.Students.ForBulkUnenrolAsync(id);

            foreach (var enrolment in student.Enrolments.Where(e => !e.IsDeleted))
            {
                await _studentService.UnenrolStudentFromClass(student.StudentId, enrolment.OfferingId);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", new { id });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Unenrol(string id, int classId)
        {
            await _studentService.UnenrolStudentFromClass(id, classId);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", new { id });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> BulkEnrol(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var student = await _unitOfWork.Students.ForEditAsync(id);

            if (student == null)
            {
                return RedirectToAction("Index");
            }

            var viewModel = await CreateViewModel<Student_BulkEnrolViewModel>();
            viewModel.StudentId = student.StudentId;
            viewModel.OfferingList = await _unitOfWork.CourseOfferings.FromGradeForBulkEnrolAsync(student.CurrentGrade);

            return View("BulkEnrol", viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> BulkEnrol(Student_BulkEnrolViewModel viewModel)
        {
            var student = await _unitOfWork.Students.ForEditAsync(viewModel.StudentId);

            if (student != null)
            {
                foreach (var offeringId in viewModel.SelectedClasses)
                {
                    var offering = await _unitOfWork.CourseOfferings.ForEnrolmentAsync(offeringId);

                    if (offering == null)
                        continue;

                    await _studentService.EnrolStudentInClass(student.StudentId, offering.Id);
                }

                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction("Details", new { id = viewModel.StudentId });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Enrol(string id, int classId)
        {
            var student = await _unitOfWork.Students.ForEditAsync(id);

            if (student == null)
            {
                return RedirectToAction("Index");
            }

            var offering = await _unitOfWork.CourseOfferings.ForEnrolmentAsync(classId);

            if (offering == null)
            {
                return RedirectToAction("Index");
            }

            if (!student.Enrolments.Any(e => e.OfferingId == classId && !e.IsDeleted))
            {
                await _studentService.EnrolStudentInClass(student.StudentId, offering.Id);
            }

            return RedirectToAction("Details", new { id });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> AbsenceSettings()
        {
            var viewModel = await CreateViewModel<Student_AbsenceSettingsViewModel>();

            var schools = await _unitOfWork.Schools.AllWithStudentsForAbsenceSettingsAsync();
            var students = await _unitOfWork.Students.AllActiveAsync();

            viewModel.SchoolList = new SelectList(schools, "Code", "Name");
            viewModel.StudentList = new SelectList(students, "StudentId", "DisplayName");
            viewModel.GradeList = new SelectList(Enum.GetValues(typeof(Grade)).Cast<Grade>().Select(v => new { Text = v.GetDisplayName(), Value = ((int)v).ToString() }).ToList(), "Value", "Text");

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [HttpPost]
        public async Task<IActionResult> AbsenceSettings(Student_AbsenceSettingsViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.StudentId) && string.IsNullOrWhiteSpace(viewModel.SchoolCode))
            {
                // Invalid entry!
                await UpdateViewModel(viewModel);
                var schools = await _unitOfWork.Schools.AllWithStudentsForAbsenceSettingsAsync();
                var students = await _unitOfWork.Students.AllActiveAsync();

                viewModel.SchoolList = new SelectList(schools, "Code", "Name");
                viewModel.GradeList = new SelectList(Enum.GetValues(typeof(Grade)).Cast<Grade>().Select(v => new { Text = v.ToString(), Value = ((int)v).ToString() }).ToList(), "Value", "Text");
                viewModel.StudentList = new SelectList(students, "StudentId", "DisplayName");

                ModelState.AddModelError("", "You must select a filter!");

                return View(viewModel);
            }

            if (!string.IsNullOrWhiteSpace(viewModel.StudentId))
            {
                await _studentService.EnableStudentAbsenceNotifications(viewModel.StudentId, viewModel.StartDate);

                await _unitOfWork.CompleteAsync();

                return RedirectToAction("Active");
            }

            if (!string.IsNullOrWhiteSpace(viewModel.SchoolCode))
            {

                var students = new List<Student>();

                if (string.IsNullOrWhiteSpace(viewModel.Grade))
                {
                    students = (await _unitOfWork.Students.ForListAsync(student => !student.IsDeleted && student.SchoolCode == viewModel.SchoolCode)).ToList();
                }
                else
                {
                    _ = Enum.TryParse(viewModel.Grade, out Grade grade);

                    students = (await _unitOfWork.Students.ForListAsync(student => !student.IsDeleted && student.SchoolCode == viewModel.SchoolCode && student.CurrentGrade == grade)).ToList();
                }

                foreach (var student in students)
                {
                    await _studentService.EnableStudentAbsenceNotifications(student.StudentId, viewModel.StartDate);
                }

                await _unitOfWork.CompleteAsync();

                return RedirectToAction("Active");
            }

            return RedirectToAction("AbsenceSettings");
        }

        public async Task<IActionResult> AbsenceDetails(string id)
        {
            var absence = await _unitOfWork.Absences.WithDetails(id);

            var vm = await CreateViewModel<Absence_Details_ViewModel>();
            vm.Absence = absence;

            return View(vm);
        }
    }
}