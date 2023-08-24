using Constellation.Application.DTOs;
using Constellation.Application.Helpers;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Core.Abstractions.Repositories;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Core.Models.Absences;
using Constellation.Core.Models.Students;
using Constellation.Core.Models.Subjects.Identifiers;
using Constellation.Core.Shared;
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
        private readonly IOfferingRepository _offeringRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentService _studentService;
        private readonly IOperationService _operationService;
        private readonly IMediator _mediator;

        public StudentsController(
            IOfferingRepository offeringRepository,
            IUnitOfWork unitOfWork, 
            IStudentService studentService,
            IOperationService operationService, 
            IMediator mediator)
            : base(mediator)
        {
            _offeringRepository = offeringRepository;
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

            foreach (var student in students)
            {
                if (student.AbsenceConfigurations.Count == 0 && student.IncludeInAbsenceNotifications)
                {
                    Result<AbsenceConfiguration> wholeRequest = AbsenceConfiguration.Create(student.StudentId, AbsenceType.Whole, DateOnly.FromDateTime(student.AbsenceNotificationStartDate.Value), null);

                    if (wholeRequest.IsSuccess)
                        student.AddAbsenceConfiguration(wholeRequest.Value);

                    Result<AbsenceConfiguration> partialRequest = AbsenceConfiguration.Create(student.StudentId, AbsenceType.Partial, DateOnly.FromDateTime(student.AbsenceNotificationStartDate.Value), null);

                    if (partialRequest.IsSuccess)
                        student.AddAbsenceConfiguration(partialRequest.Value);

                    await _unitOfWork.CompleteAsync();
                }
            }

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
        public async Task<IActionResult> Unenrol(string id, Guid classId)
        {
            OfferingId offeringId = OfferingId.FromValue(classId);

            if (offeringId is null)
                return RedirectToAction("Details", new { id });

            await _studentService.UnenrolStudentFromClass(id, offeringId);
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
            viewModel.OfferingList = await _offeringRepository.GetActiveByGrade(student.CurrentGrade);

            return View("BulkEnrol", viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> BulkEnrol(Student_BulkEnrolViewModel viewModel)
        {
            var student = await _unitOfWork.Students.ForEditAsync(viewModel.StudentId);

            if (student != null)
            {
                foreach (var classId in viewModel.SelectedClasses)
                {
                    OfferingId offeringId = OfferingId.FromValue(classId);

                    var offering = await _offeringRepository.GetById(offeringId);

                    if (offering == null)
                        continue;

                    await _studentService.EnrolStudentInClass(student.StudentId, offering.Id);
                }

                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction("Details", new { id = viewModel.StudentId });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Enrol(string id, Guid classId)
        {
            var student = await _unitOfWork.Students.ForEditAsync(id);

            if (student == null)
            {
                return RedirectToAction("Index");
            }

            OfferingId offeringId = OfferingId.FromValue(classId);

            var offering = await _offeringRepository.GetById(offeringId);

            if (offering == null)
            {
                return RedirectToAction("Index");
            }

            if (!student.Enrolments.Any(e => e.OfferingId == offeringId && !e.IsDeleted))
            {
                await _studentService.EnrolStudentInClass(student.StudentId, offering.Id);
            }

            return RedirectToAction("Details", new { id });
        }
    }
}