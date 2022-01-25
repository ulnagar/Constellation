using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Areas.Portal.Models.Lessons;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Controllers
{
    [Area("Portal")]
    public class LessonsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly ILessonService _lessonService;
        private readonly ISchoolContactService _schoolContactService;
        private readonly IOperationService _operationsService;

        public LessonsController(IUnitOfWork unitOfWork, IAuthService authService,
            ILessonService lessonService, ISchoolContactService schoolContactService,
            IOperationService operationsService)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _lessonService = lessonService;
            _schoolContactService = schoolContactService;
            _operationsService = operationsService;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole(AuthRoles.LessonsUser))
                    return LocalRedirect("/Portal/Lessons/Teacher");

                if (User.IsInRole(AuthRoles.LessonsEditor) || User.IsInRole(AuthRoles.Admin))
                    return RedirectToAction("AdminLessons");
            }

            var vm = new ErrorViewModel
            {
                MainMessage = "There is an error with your account. Please contact the Technology Support Team on 1300 610 733."
            };

            return View("Error", vm);
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Lessons")]
        public async Task<IActionResult> AdminLessons(string filter = "")
        {
            var vm = new AdminViewModel { filter = filter };

            var lessons = await _unitOfWork.Lessons.GetAllForPortalAdmin();

            if (lessons == null)
            {
                return View(vm);
            }

            switch (filter)
            {
                case "":
                    foreach (var lesson in lessons.Where(l => l.Rolls.Any(r => r.Status == LessonStatus.Active)))
                    {
                        vm.Lessons.Add(AdminViewModel.LessonDto.ConvertFromLesson(lesson));
                    }
                    break;
                case "overdue":
                    foreach (var lesson in lessons.Where(l => l.Rolls.Any(r => r.Status == LessonStatus.Active) && l.DueDate < DateTime.Today))
                    {
                        vm.Lessons.Add(AdminViewModel.LessonDto.ConvertFromLesson(lesson));
                    }
                    break;
                case "future":
                    foreach (var lesson in lessons.Where(l => l.DueDate >= DateTime.Today))
                    {
                        vm.Lessons.Add(AdminViewModel.LessonDto.ConvertFromLesson(lesson));
                    }
                    break;
                case "completed":
                    foreach (var lesson in lessons.Where(l => l.Rolls.All(r => r.Status != LessonStatus.Active)))
                    {
                        vm.Lessons.Add(AdminViewModel.LessonDto.ConvertFromLesson(lesson));
                    }
                    break;
            }

            var schools = await _unitOfWork.Schools.ForSelectionAsync();
            var students = await _unitOfWork.Students.ForSelectionListAsync();

            vm.SearchViewModel = new AdminSearchViewModel
            {
                SchoolList = new SelectList(schools, "Code", "Name"),
                StudentList = new SelectList(students, "StudentId", "DisplayName")
            };

            return View(vm);
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/Lessons/Search")]
        public async Task<IActionResult> AdminLessonSearch(AdminSearchViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AdminLessons");
            }

            var vm = new AdminViewModel { SearchResult = true };

            var schools = await _unitOfWork.Schools.ForSelectionAsync();
            var students = await _unitOfWork.Students.ForSelectionListAsync();

            vm.SearchViewModel = new AdminSearchViewModel
            {
                SchoolList = new SelectList(schools, "Code", "Name"),
                StudentList = new SelectList(students, "StudentId", "DisplayName")
            };

            var lessons = await _unitOfWork.Lessons.GetAllForPortalAdmin();

            if (lessons == null)
            {
                return View("AdminLessons", vm);
            }

            if (!string.IsNullOrEmpty(viewModel.SchoolCode))
            {
                foreach (var lesson in lessons.Where(l => l.Rolls.Any(r => r.SchoolCode == viewModel.SchoolCode)))
                {
                    var roll = lesson.Rolls.First(l => l.SchoolCode == viewModel.SchoolCode);
                    vm.Lessons.Add(AdminViewModel.LessonDto.ConvertFromRoll(roll));
                }

                return View("AdminLessons", vm);
            }

            if (!string.IsNullOrEmpty(viewModel.StudentId))
            {
                foreach (var lesson in lessons.Where(l => l.Rolls.Any(r => r.Attendance.Any(a => a.StudentId == viewModel.StudentId))))
                {
                    var roll = lesson.Rolls.First(r => r.Attendance.Any(a => a.StudentId == viewModel.StudentId));
                    vm.Lessons.Add(AdminViewModel.LessonDto.ConvertFromRoll(roll));
                }

                return View("AdminLessons", vm);
            }

            return RedirectToAction("AdminLessons");
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [HttpGet]
        [Route("Admin/Lessons/Details/{id:guid}")]
        public async Task<IActionResult> AdminLessonDetails(Guid id)
        {
            var lesson = await _unitOfWork.Lessons.GetWithDetailsForLessonsPortal(id);
            var vm = AdminDetailViewModel.ConvertFromLesson(lesson);

            return View(vm);
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Roll/{id:guid}")]
        public async Task<IActionResult> AdminRollDetails(Guid id)
        {
            var roll = await _unitOfWork.Lessons.GetRollForPortal(id);

            var vm = new RollViewModel
            {
                Roll = LessonRollDto.ConvertFromRoll(roll)
            };

            if (roll.LessonDate.HasValue && roll.SchoolContactId.HasValue)
                vm.Roll.TeacherName = roll.SchoolContact.DisplayName;

            if (roll.LessonDate.HasValue && !roll.SchoolContactId.HasValue)
                vm.Roll.TeacherName = "Submitted by Admin";

            foreach (var attend in roll.Attendance)
                vm.Roll.Attendance.Add(LessonRollDto.StudentRollDto.ConvertFromRollAttendance(attend));

            return View(vm);
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Roll/{id:guid}/Submit")]
        public async Task<IActionResult> AdminRollSubmit(Guid id)
        {
            var roll = await _unitOfWork.Lessons.GetRollForPortal(id);

            var vm = new RollViewModel
            {
                Roll = LessonRollDto.ConvertFromRoll(roll)
            };

            if (roll.LessonDate.HasValue && roll.SchoolContactId.HasValue)
                vm.Roll.TeacherName = roll.SchoolContact.DisplayName;

            if (roll.LessonDate.HasValue && !roll.SchoolContactId.HasValue)
                vm.Roll.TeacherName = "Submitted by Admin";

            foreach (var attend in roll.Attendance)
                vm.Roll.Attendance.Add(LessonRollDto.StudentRollDto.ConvertFromRollAttendance(attend));

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Roll/{id:guid}/Submit")]
        public async Task<IActionResult> AdminRollSubmit(RollViewModel vm)
        {
            if (vm.Roll.LessonDate > DateTime.Today)
                ModelState.AddModelError("LessonDate", "Cannot deliver a lesson in the future!");

            if (!ModelState.IsValid)
                return RedirectToAction("Roll", vm.Roll.RollId);

            var serviceSuccess = await GetStaffMember();

            if (serviceSuccess.Success == false)
            {
                var errorVm = new ErrorViewModel
                {
                    MainMessage = "There is an error with your account. Please contact the Technology Support Team on 1300 610 733.",
                    SecondaryMessages = serviceSuccess.Errors
                };

                return View("Error", errorVm);
            }

            await _lessonService.SubmitLessonRoll(vm.Roll);

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("AdminRollDetails", vm.Roll.RollId);
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Roll/{id:guid}/Cancel")]
        public async Task<IActionResult> AdminRollCancel(Guid id)
        {
            var roll = await _unitOfWork.Lessons.GetRollForPortal(id);

            if (roll.SubmittedDate == null)
            {
                roll.Status = LessonStatus.Cancelled;
                await _unitOfWork.CompleteAsync();

                return RedirectToAction("AdminLessonDetails", new { id = roll.LessonId });
            }

            var errorVm = new ErrorViewModel
            {
                MainMessage = "Cannot cancel roll that has been submitted!"
            };

            return View("Error", errorVm);
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Roll/{id:guid}/UnCancel")]
        public async Task<IActionResult> AdminRollUnCancel(Guid id)
        {
            var roll = await _unitOfWork.Lessons.GetRollForPortal(id);

            if (roll.Status == LessonStatus.Cancelled)
            {
                roll.Status = LessonStatus.Active;
                await _unitOfWork.CompleteAsync();

                return RedirectToAction("AdminLessonDetails", new { id = roll.LessonId });
            }

            var errorVm = new ErrorViewModel
            {
                MainMessage = "Cannot un-cancel roll that has been not been cancelled!"
            };

            return View("Error", errorVm);
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Teachers")]
        public async Task<IActionResult> AdminTeachers()
        {
            // Flatten contacts and roles into unique combinations of both.

            var vm = new AdminTeachersViewModel();
            var contacts = await _unitOfWork.SchoolContacts.ScienceTeachersForLessonsPortalAdmin();
            foreach (var contact in contacts)
            {
                foreach (var role in contact.Assignments.Where(assignment => assignment.Role == SchoolContactRole.SciencePrac && !assignment.IsDeleted))
                {
                    vm.Teachers.Add(AdminTeachersViewModel.TeacherDto.ConvertFromContactRole(role));
                }
            }

            return View(vm);
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Teachers/Audit/{id?}")]
        public async Task<IActionResult> AdminTeacherAudit(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("AdminTeachers");
            }

            var contact = await _unitOfWork.SchoolContacts.ForAudit(id.Value);

            var audit = await _authService.VerifyContactAccess(contact.EmailAddress);

            var vm = new AdminTeacherAuditViewModel
            {
                AuditResult = audit
            };

            return View(vm);
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Lessons/Edit/{id?}")]
        public async Task<IActionResult> AdminUpsertLesson(string id)
        {
            var courses = await _unitOfWork.Courses.ForSelectionAsync();

            if (string.IsNullOrWhiteSpace(id))
            {
                var vm = new AdminUpsertLessonViewModel
                {
                    CourseList = new SelectList(courses, "Id", "Name", null, "Grade"),
                    Lesson = new LessonDto
                    {
                        DueDate = DateTime.Today
                    }
                };

                return View(vm);
            }
            else
            {
                var guidId = new Guid(id);
                var lesson = await _unitOfWork.Lessons.GetForEdit(guidId);

                if (lesson.Rolls.Any(roll => roll.LessonDate.HasValue))
                {
                    // This lesson has already had a submitted roll and cannot be modified
                    return RedirectToAction("AdminLessons");
                }

                if (lesson.DueDate < DateTime.Today)
                {
                    // This lesson was due in the past and cannot be modified
                    return RedirectToAction("AdminLessons");
                }

                var vm = new AdminUpsertLessonViewModel
                {
                    CourseList = new SelectList(courses, "Id", "Name", null, "Grade"),
                    Lesson = new LessonDto
                    {
                        Id = lesson.Id,
                        Name = lesson.Name,
                        DueDate = lesson.DueDate,
                        CourseId = lesson.Offerings.First().CourseId,
                        DoNotGenerateRolls = lesson.DoNotGenerateRolls
                    }
                };

                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Lessons/Edit")]
        public async Task<IActionResult> AdminUpsertLesson(AdminUpsertLessonViewModel vm)
        {
            if (vm.Lesson.CourseId == 0)
            {
                ModelState.AddModelError("CourseId", "You must select a course!");
                return View("AdminUpsertLesson", vm);
            }

            if (!ModelState.IsValid)
            {
                var courses = await _unitOfWork.Courses.ForSelectionAsync();
                vm.CourseList = new SelectList(courses, "Id", "Name", vm.Lesson.CourseId, "Grade");
                return View("AdminUpsertLesson", vm);
            }

            if (vm.Lesson.Id == new Guid())
            {
                await _lessonService.CreateNewLesson(vm.Lesson);

                await _unitOfWork.CompleteAsync();
                return RedirectToAction("AdminLessons");
            }
            else
            {
                var lesson = await _unitOfWork.Lessons.GetForEdit(vm.Lesson.Id);

                if (lesson.Rolls.Any(roll => roll.SubmittedDate != null))
                {
                    ModelState.AddModelError("Id", "You cannot modify a lesson that has marked rolls attached!");
                    return View("AdminUpsertLesson", vm);
                }

                await _lessonService.UpdateExistingLesson(vm.Lesson);

                await _unitOfWork.CompleteAsync();
                return RedirectToAction("AdminLessons");
            }
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Lessons/Delete/{id}")]
        public async Task<IActionResult> AdminDeleteLesson(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("AdminLessons");
            }
            else
            {
                var guidId = new Guid(id);
                var lesson = await _unitOfWork.Lessons.GetForEdit(guidId);

                _unitOfWork.Remove(lesson);
                await _unitOfWork.CompleteAsync();

                return RedirectToAction("AdminLessons");
            }
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Teachers/Delete/{id?}")]
        public async Task<IActionResult> AdminDeleteTeachers(int? id)
        {
            // value of ID is the School Contact id, not the school contact id
            if (id.HasValue)
            {
                await _schoolContactService.RemoveRole(id.Value);

                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction("AdminCoordinators");
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Teachers/Edit/{id?}")]
        public async Task<IActionResult> AdminUpsertTeacher(int? id)
        {
            // value of ID is the ROLE ASSIGNMENT id, not the school contact id

            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            if (!id.HasValue)
            {
                var vm = new AdminUpsertTeacherViewModel
                {
                    SchoolList = new SelectList(schools, "Code", "Name")
                };

                return View(vm);
            }
            else
            {
                var role = await _unitOfWork.SchoolContactRoles.WithDetails(id.Value);
                var vm = AdminUpsertTeacherViewModel.ConvertFromContactRole(role);

                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Teachers/Edit")]
        public async Task<IActionResult> AdminUpsertTeacher(AdminUpsertTeacherViewModel vm)
        {
            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            if (!ModelState.IsValid)
            {
                vm.SchoolList = new SelectList(schools, "Code", "Name", vm.SchoolCode);
                return View("AdminUpsertTeacher", vm);
            }

            if (vm.Id.HasValue)
            {
                // Existing Coordinator needs to be updated
                var contactResource = new SchoolContactDto
                {
                    FirstName = vm.FirstName,
                    LastName = vm.LastName,
                    PhoneNumber = vm.PhoneNumber,
                    EmailAddress = vm.EmailAddress
                };

                var contactResult = await _schoolContactService.UpdateContact(contactResource);

                var roleResource = new SchoolContactRoleDto
                {
                    Role = SchoolContactRole.SciencePrac,
                    SchoolCode = vm.SchoolCode,
                    SchoolContactId = vm.Id.Value
                };
                var roleResult = await _schoolContactService.UpdateRole(roleResource);

                if (contactResult.Success && roleResult.Success)
                {
                    await _unitOfWork.CompleteAsync();
                    return RedirectToAction("AdminTeachers");
                }
                else
                {
                    vm.SchoolList = new SelectList(schools, "Code", "Name", vm.SchoolCode);
                    return View(vm);
                }
            }
            else
            {
                // New Coordinator (maybe)
                // Check to see if another coordinator with same email is registered.
                var contact = await _unitOfWork.SchoolContacts.FromEmailForExistCheck(vm.EmailAddress);

                if (contact == null)
                {
                    var contactResource = new SchoolContactDto
                    {
                        FirstName = vm.FirstName,
                        LastName = vm.LastName,
                        PhoneNumber = vm.PhoneNumber,
                        EmailAddress = vm.EmailAddress
                    };

                    var contactResult = await _schoolContactService.CreateContact(contactResource);
                    if (contactResult.Success)
                    {
                        await _unitOfWork.CompleteAsync();
                        await _operationsService.CreateContactAddedMSTeamAccess(contactResult.Entity.Id);
                        contact = contactResult.Entity;
                    }
                    else
                    {
                        vm.SchoolList = new SelectList(schools, "Code", "Name", vm.SchoolCode);
                        return View(vm);
                    }
                }

                if (contact != null)
                {
                    if (contact.IsDeleted)
                    {
                        await _schoolContactService.UndeleteContact(contact.Id);

                        await _unitOfWork.CompleteAsync();
                    }

                    var roleResource = new SchoolContactRoleDto
                    {
                        SchoolContactId = contact.Id,
                        SchoolCode = vm.SchoolCode,
                        Role = SchoolContactRole.SciencePrac
                    };

                    var roleResult = await _schoolContactService.CreateRole(roleResource);
                    if (roleResult.Success)
                    {
                        await _authService.AuditSchoolContactUsers();

                        await _unitOfWork.CompleteAsync();
                        return RedirectToAction("AdminTeachers");
                    }
                    else
                    {
                        vm.SchoolList = new SelectList(schools, "Code", "Name", vm.SchoolCode);
                        return View(vm);
                    }
                }

                vm.SchoolList = new SelectList(schools, "Code", "Name");
                return View(vm);
            }
        }

        public async Task<IActionResult> AdminRepairTeacherAccount(int id)
        {
            await _authService.RepairSchoolContactUser(id);

            return RedirectToAction("AdminTeachers");
        }

        private async Task<ServiceOperationResult<Staff>> GetStaffMember()
        {
            var result = new ServiceOperationResult<Staff>();

            var username = User.Identity.Name;
            var staffUser = await _unitOfWork.Staff.FromEmailForExistCheck(username);

            if (staffUser == null)
            {
                result.Errors.Add($"No user could be found with the email address {username}");
                return result;
            }

            result.Success = true;
            result.Entity = staffUser;
            return result;
        }

    }
}