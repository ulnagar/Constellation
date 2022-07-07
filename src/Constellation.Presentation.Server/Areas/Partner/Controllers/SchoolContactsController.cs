using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Enums;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.User)]
    public class SchoolContactsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly ISchoolContactService _schoolContactService;
        private readonly IOperationService _operationService;

        public SchoolContactsController(IUnitOfWork unitOfWork, IAuthService authService,
            ISchoolContactService schoolContactService, IOperationService operationService)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _schoolContactService = schoolContactService;
            _operationService = operationService;
        }

        // GET: Partner/SchoolContact
        public IActionResult Index()
        {
            return RedirectToAction("All");
        }

        public async Task<IActionResult> All()
        {
            var withRole = await _unitOfWork.SchoolContacts.AllWithActiveRoleAsync();
            var withoutRole = await _unitOfWork.SchoolContacts.AllWithoutActiveRoleAsync();

            var contactList = withoutRole.Select(SchoolStaff_ViewModel.ContactDto.ConvertFromContact).ToList();
            contactList.AddRange(withRole.SelectMany(SchoolStaff_ViewModel.ContactDto.ConvertFromContactWithRole));

            var viewModel = await CreateViewModel<SchoolStaff_ViewModel>();
            viewModel.Contacts = contactList.OrderBy(contact => contact.Name).ToList();
            viewModel.RoleList = await _unitOfWork.SchoolContactRoles.ListOfRolesForSelectionAsync();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> FromGrade(Grade id)
        {
            var withRole = await _unitOfWork.SchoolContactRoles.AllCurrentFromGrade(id);

            var contactList = withRole.Select(SchoolStaff_ViewModel.ContactDto.ConvertFromAssignment).ToList();

            var viewModel = await CreateViewModel<SchoolStaff_ViewModel>();
            viewModel.Contacts = contactList.OrderBy(contact => contact.Name).ToList();
            viewModel.RoleList = await _unitOfWork.SchoolContactRoles.ListOfRolesForSelectionAsync();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithRole(string role)
        {
            var withRole = await _unitOfWork.SchoolContactRoles.AllCurrentWithRole(role);

            var contactList = withRole.Select(SchoolStaff_ViewModel.ContactDto.ConvertFromAssignment).ToList();

            var viewModel = await CreateViewModel<SchoolStaff_ViewModel>();
            viewModel.Contacts = contactList.OrderBy(contact => contact.Name).ToList();
            viewModel.RoleList = await _unitOfWork.SchoolContactRoles.ListOfRolesForSelectionAsync();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Search(string[] role, int[] grade)
        {
            var filteredContacts = new List<SchoolContactRole>();

            foreach (Grade selectedGrade in grade)
            {
                filteredContacts.AddRange(await _unitOfWork.SchoolContactRoles.AllCurrentFromGrade(selectedGrade));
            }

            if (role != null && filteredContacts.Any())
            {
                filteredContacts = filteredContacts.Where(r => role.Contains(r.Role)).ToList();
            }
            else if (role != null)
            {
                foreach (var selectedRole in role)
                {
                    filteredContacts.AddRange(await _unitOfWork.SchoolContactRoles.AllCurrentWithRole(selectedRole));
                }
            }

            var contactList = filteredContacts.Distinct().Select(SchoolStaff_ViewModel.ContactDto.ConvertFromAssignment).ToList();

            var viewModel = await CreateViewModel<SchoolStaff_ViewModel>();
            viewModel.Contacts = contactList.OrderBy(contact => contact.Name).ToList();
            viewModel.RoleList = await _unitOfWork.SchoolContactRoles.ListOfRolesForSelectionAsync();

            return View("Index", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> AddAssignment(int id)
        {
            var contact = await _unitOfWork.SchoolContacts.FromIdForExistCheck(id);
            var roles = await _unitOfWork.SchoolContactRoles.ListOfRolesForSelectionAsync();

            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var viewModel = await CreateViewModel<Contacts_AssignmentViewModel>();
            viewModel.SchoolList = new SelectList(schools, "Code", "Name");
            viewModel.ContactRole = new SchoolContactRoleDto
            {
                SchoolContactId = id,
                SchoolContactName = contact.DisplayName
            };
            viewModel.RoleList = new SelectList(roles);
            viewModel.ReturnUrl = Request.GetTypedHeaders().Referer.ToString();

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> _AddAssignment(Contacts_AssignmentViewModel viewModel)
        {
            await _schoolContactService.CreateRole(viewModel.ContactRole);
            await _unitOfWork.CompleteAsync();

            if (viewModel.ReturnUrl == null)
                return Redirect(Request.GetTypedHeaders().Referer.ToString());
            else 
                return Redirect(viewModel.ReturnUrl);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Update(int id)
        {
            if (id == 0)
            {
                RedirectToAction("Index");
            }

            var contact = await _unitOfWork.SchoolContacts.ForEditAsync(id);

            if (contact == null)
            {
                RedirectToAction("Index");
            }

            var viewModel = await CreateViewModel<SchoolStaff_UpdateViewModel>();
            viewModel.Contact = new SchoolContactDto
            {
                Id = id,
                FirstName = contact.FirstName,
                LastName = contact.LastName,
                EmailAddress = contact.EmailAddress,
                PhoneNumber = contact.PhoneNumber,
                SelfRegistered = contact.SelfRegistered
            };

            viewModel.IsNew = false;

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SchoolStaff_UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await UpdateViewModel(viewModel);
                return View("Update", viewModel);
            }

            if (viewModel.IsNew)
            {
                var result = await _schoolContactService.CreateContact(viewModel.Contact);

                if (!result.Success)
                {
                    await UpdateViewModel(viewModel);
                    foreach (var error in result.Errors)
                        ModelState.AddModelError("", error);
                    return View("Update", viewModel);
                }

                await _unitOfWork.CompleteAsync();

                await _operationService.CreateContactAddedMSTeamAccess(result.Entity.Id);

                var user = new UserTemplateDto
                {
                    FirstName = result.Entity.FirstName.Trim(' '),
                    LastName = result.Entity.LastName.Trim(' '),
                    Email = result.Entity.EmailAddress.Trim(' '),
                    Username = result.Entity.EmailAddress.Trim(' '),
                    IsSchoolContact = true,
                    SchoolContactId = result.Entity.Id
                };

                await _authService.CreateUser(user);

                if (!string.IsNullOrWhiteSpace(viewModel.ContactRole.SchoolCode) && !string.IsNullOrWhiteSpace(viewModel.ContactRole.Role))
                {
                    viewModel.ContactRole.SchoolContactId = result.Entity.Id;

                    await _schoolContactService.CreateRole(viewModel.ContactRole);
                }
            }
            else
            {
                var contact = await _unitOfWork.SchoolContacts.ForEditAsync(viewModel.Contact.Id.Value);

                var checkEmail = contact.EmailAddress;

                var result = await _schoolContactService.UpdateContact(viewModel.Contact);

                if (!result.Success)
                {
                    await UpdateViewModel(viewModel);
                    return View("Update", viewModel);
                }

                var newUser = new UserTemplateDto
                {
                    FirstName = result.Entity.FirstName.Trim(' '),
                    LastName = result.Entity.LastName.Trim(' '),
                    Email = result.Entity.EmailAddress.Trim(' '),
                    Username = result.Entity.EmailAddress.Trim(' '),
                    IsSchoolContact = true,
                    SchoolContactId = result.Entity.Id
                };

                await _authService.UpdateUser(checkEmail, newUser);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Create()
        {
            var roles = await _unitOfWork.SchoolContactRoles.ListOfRolesForSelectionAsync();
            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var viewModel = await CreateViewModel<SchoolStaff_UpdateViewModel>();
            viewModel.IsNew = true;
            viewModel.ContactRole = new();
            viewModel.SchoolList = new SelectList(schools, "Code", "Name");
            viewModel.RoleList = new SelectList(roles);
            
            return View("Update", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var role = await _unitOfWork.SchoolContactRoles.WithDetails(id);

            if (role.SchoolContact.Assignments.Count(assign => !assign.IsDeleted) == 1)
            {
                // This is the last role. User should be updated to remove "IsSchoolContact" flag.
                var newUser = new UserTemplateDto
                {
                    FirstName = role.SchoolContact.FirstName,
                    LastName = role.SchoolContact.LastName,
                    Email = role.SchoolContact.EmailAddress,
                    Username = role.SchoolContact.EmailAddress,
                    IsSchoolContact = false
                };

                await _authService.UpdateUser(role.SchoolContact.EmailAddress, newUser);

                // Also remove user from the MS Teams
                await _operationService.RemoveContactAddedMSTeamAccess(role.SchoolContactId);
            }

            await _schoolContactService.RemoveRole(id);
            await _unitOfWork.CompleteAsync();

            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }
    }
}