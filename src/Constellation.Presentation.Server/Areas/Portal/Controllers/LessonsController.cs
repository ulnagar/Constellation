using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContactWithRole;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Areas.Portal.Models.Lessons;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Portal.Controllers
{
    [Area("Portal")]
    [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
    public class LessonsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly ILessonService _lessonService;
        private readonly ISchoolContactService _schoolContactService;
        private readonly IMediator _mediator;

        public LessonsController(IUnitOfWork unitOfWork, IAuthService authService,
            ILessonService lessonService, ISchoolContactService schoolContactService,
            IMediator mediator)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _lessonService = lessonService;
            _schoolContactService = schoolContactService;
            _mediator = mediator;
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Teachers")]
        public async Task<IActionResult> AdminTeachers()
        {
            // Flatten contacts and roles into unique combinations of both.

            var vm = await CreateViewModel<AdminTeachersViewModel>();
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

            var vm = await CreateViewModel<AdminTeacherAuditViewModel>();
            vm.AuditResult = audit;

            return View(vm);
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Teachers/Delete/{id?}")]
        public async Task<IActionResult> AdminDeleteTeacher(int? id)
        {
            // value of ID is the ROLE id, not the school contact id
            if (id.HasValue)
            {
                await _schoolContactService.RemoveRole(id.Value);

                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction("AdminTeachers");
        }

        [Roles(AuthRoles.Admin, AuthRoles.LessonsEditor)]
        [Route("Admin/Teachers/Edit/{id?}")]
        public async Task<IActionResult> AdminUpsertTeacher(int? id)
        {
            // value of ID is the ROLE ASSIGNMENT id, not the school contact id

            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            if (!id.HasValue)
            {
                var vm = await CreateViewModel<AdminUpsertTeacherViewModel>();
                vm.SchoolList = new SelectList(schools, "Code", "Name");

                return View(vm);
            }
            else
            {
                var role = await _unitOfWork.SchoolContactRoles.WithDetails(id.Value);
                var vm = AdminUpsertTeacherViewModel.ConvertFromContactRole(role);
                await UpdateViewModel(vm);

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
                await UpdateViewModel(vm);
                vm.SchoolList = new SelectList(schools, "Code", "Name", vm.SchoolCode);
                return View("AdminUpsertTeacher", vm);
            }

            if (vm.Id.HasValue)
            {
                // Existing Coordinator needs to be updated
                var contactResource = new SchoolContactDto
                {
                    FirstName = vm.FirstName.Trim(' '),
                    LastName = vm.LastName.Trim(' '),
                    PhoneNumber = vm.PhoneNumber,
                    EmailAddress = vm.EmailAddress.Trim(' ')
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
                    await UpdateViewModel(vm);
                    vm.SchoolList = new SelectList(schools, "Code", "Name", vm.SchoolCode);
                    return View(vm);
                }
            }
            else
            {
                await _mediator.Send(new CreateContactWithRoleCommand(
                    vm.FirstName,
                    vm.LastName,
                    vm.EmailAddress, 
                    vm.PhoneNumber,
                    SchoolContactRole.SciencePrac,
                    vm.SchoolCode,
                    false));

                return RedirectToAction("AdminTeachers");
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