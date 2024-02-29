using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.SchoolContacts.CreateContact;
using Constellation.Application.SchoolContacts.CreateContactRoleAssignment;
using Constellation.Application.SchoolContacts.CreateContactWithRole;
using Constellation.Core.Enums;
using Constellation.Core.Models.SchoolContacts;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Partner.Controllers
{
    using Core.Models;
    using Core.Models.SchoolContacts.Identifiers;
    using Core.Models.SchoolContacts.Repositories;
    using Core.Shared;

    [Area("Partner")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class SchoolContactsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly ISchoolContactRepository _contactRepository;
        private readonly IOperationService _operationService;
        private readonly IMediator _mediator;

        public SchoolContactsController(
            IUnitOfWork unitOfWork, 
            IAuthService authService,
            ISchoolContactRepository contactRepository,
            IOperationService operationService,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _authService = authService;
            _contactRepository = contactRepository;
            _operationService = operationService;
            _mediator = mediator;
        }

        // GET: Partner/SchoolContact
        public IActionResult Index()
        {
            return RedirectToAction("All");
        }

        public async Task<IActionResult> All()
        {
            var contacts = await _contactRepository.GetAll();

            var withRole = contacts.Where(contact => contact.Assignments.Any(role => !role.IsDeleted));
            var withoutRole = contacts.Where(contact => contact.Assignments.All(role => role.IsDeleted));

            var contactList = withoutRole.Select(SchoolStaff_ViewModel.ContactDto.ConvertFromContact).ToList();
            contactList.AddRange(withRole.SelectMany(SchoolStaff_ViewModel.ContactDto.ConvertFromContactWithRole));

            var viewModel = new SchoolStaff_ViewModel();
            viewModel.Contacts = contactList.OrderBy(contact => contact.Name).ToList();
            viewModel.RoleList = await _contactRepository.GetAvailableRoleList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> FromGrade(Grade grade)
        {
            List<SchoolContact> withRole = await _contactRepository.GetByGrade(grade);

            List<SchoolStaff_ViewModel.ContactDto> contactList = withRole.SelectMany(SchoolStaff_ViewModel.ContactDto.ConvertFromContactWithRole).ToList();

            SchoolStaff_ViewModel viewModel = new()
            {
                Contacts = contactList.OrderBy(contact => contact.Name).ToList(), 
                RoleList = await _contactRepository.GetAvailableRoleList()
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithRole(string role)
        {
            List<SchoolContact> withRole = await _contactRepository.GetAllByRole(role);

            List<SchoolStaff_ViewModel.ContactDto> contactList = new();

            foreach (SchoolContact contact in withRole)
            {
                contactList.AddRange(contact.Assignments
                    .Where(assignment => 
                        !assignment.IsDeleted && 
                        assignment.Role == role)
                    .Select(assignment => 
                        SchoolStaff_ViewModel.ContactDto.ConvertFromAssignment(contact, assignment)));
            }

            SchoolStaff_ViewModel viewModel = new()
            {
                Contacts = contactList.OrderBy(contact => contact.Name).ToList(), 
                RoleList = await _contactRepository.GetAvailableRoleList()
            };

            return View("Index", viewModel);
        }
        
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> AddAssignment(Guid id)
        {
            SchoolContactId contactId = SchoolContactId.FromValue(id);

            SchoolContact contact = await _contactRepository.GetById(contactId);
            List<string> roles = await _contactRepository.GetAvailableRoleList();

            ICollection<School> schools = await _unitOfWork.Schools.ForSelectionAsync();

            Contacts_AssignmentViewModel viewModel = new()
            {
                SchoolList = new(schools, "Code", "Name"), 
                ContactRole = new()
                {
                    SchoolContactId = id,
                    SchoolContactName = contact.DisplayName
                },
                RoleList = new(roles),
                ReturnUrl = Request.GetTypedHeaders().Referer?.ToString()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> _AddAssignment(Contacts_AssignmentViewModel viewModel)
        {
            SchoolContactId contactId = SchoolContactId.FromValue(viewModel.ContactRole.SchoolContactId);

            await _mediator.Send(new CreateContactRoleAssignmentCommand(
                contactId,
                viewModel.ContactRole.SchoolCode,
                viewModel.ContactRole.Role,
                string.Empty));

            if (viewModel.ReturnUrl == null)
                return RedirectToAction("All");
            else 
                return Redirect(viewModel.ReturnUrl);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Update(Guid id)
        {
            if (id == default)
            {
                RedirectToAction("Index");
            }

            SchoolContactId contactId = SchoolContactId.FromValue(id);

            SchoolContact contact = await _contactRepository.GetById(contactId);

            if (contact is null)
            {
                RedirectToAction("Index");
            }

            SchoolStaff_UpdateViewModel viewModel = new() { 
                Contact = new SchoolContactDto
                {
                    Id = id,
                    FirstName = contact.FirstName.Trim(),
                    LastName = contact.LastName.Trim(),
                    EmailAddress = contact.EmailAddress.Trim(),
                    PhoneNumber = (string.IsNullOrWhiteSpace(contact.PhoneNumber) ? string.Empty : contact.PhoneNumber.Trim()),
                    SelfRegistered = contact.SelfRegistered
                },
                IsNew = false
            };

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(SchoolStaff_UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Update", viewModel);
            }

            viewModel.Contact.FirstName = (string.IsNullOrWhiteSpace(viewModel.Contact.FirstName) ? string.Empty : viewModel.Contact.FirstName.Trim());
            viewModel.Contact.LastName = (string.IsNullOrWhiteSpace(viewModel.Contact.LastName) ? string.Empty : viewModel.Contact.LastName.Trim());
            viewModel.Contact.PhoneNumber = (string.IsNullOrWhiteSpace(viewModel.Contact.PhoneNumber) ? string.Empty : viewModel.Contact.PhoneNumber.Trim());
            viewModel.Contact.EmailAddress = (string.IsNullOrWhiteSpace(viewModel.Contact.EmailAddress) ? string.Empty : viewModel.Contact.EmailAddress.Trim());

            viewModel.ContactRole.Role = (string.IsNullOrWhiteSpace(viewModel.ContactRole.Role) ? string.Empty : viewModel.ContactRole.Role.Trim());

            if (viewModel.IsNew)
            {
                if (string.IsNullOrWhiteSpace(viewModel.ContactRole.SchoolCode))
                {
                    await _mediator.Send(new CreateContactCommand(
                        viewModel.Contact.FirstName,
                        viewModel.Contact.LastName,
                        viewModel.Contact.EmailAddress,
                        viewModel.Contact.PhoneNumber,
                        false));
                } else
                {
                    await _mediator.Send(new CreateContactWithRoleCommand(
                        viewModel.Contact.FirstName,
                        viewModel.Contact.LastName,
                        viewModel.Contact.EmailAddress,
                        viewModel.Contact.PhoneNumber,
                        viewModel.ContactRole.Role,
                        viewModel.ContactRole.SchoolCode,
                        string.Empty,
                        false));
                }
            }
            else
            {
                SchoolContactId contactId = SchoolContactId.FromValue(viewModel.Contact.Id!.Value);

                SchoolContact contact = await _contactRepository.GetById(contactId);

                string checkEmail = contact.EmailAddress;

                Result result = contact.Update(
                    viewModel.Contact.FirstName,
                    viewModel.Contact.LastName,
                    viewModel.Contact.EmailAddress,
                    viewModel.Contact.PhoneNumber);

                if (result.IsFailure)
                {
                    return View("Update", viewModel);
                }
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Create()
        {
            var roles = await _contactRepository.GetAvailableRoleList();
            var schools = await _unitOfWork.Schools.ForSelectionAsync();

            var viewModel = new SchoolStaff_UpdateViewModel();
            viewModel.IsNew = true;
            viewModel.ContactRole = new();
            viewModel.SchoolList = new SelectList(schools, "Code", "Name");
            viewModel.RoleList = new SelectList(roles);
            
            return View("Update", viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> DeleteAssignment(Guid contactGuid, Guid roleGuid)
        {
            SchoolContactId contactId = SchoolContactId.FromValue(contactGuid);
            SchoolContactRoleId roleId = SchoolContactRoleId.FromValue(roleGuid);

            SchoolContact contact = await _contactRepository.GetById(contactId);
            contact.RemoveRole(roleId);

            await _unitOfWork.CompleteAsync();

            return Redirect(Request.GetTypedHeaders().Referer.ToString());
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> RepairUserAccount(Guid id)
        {
            SchoolContactId contactId = SchoolContactId.FromValue(id);

            await _authService.RepairSchoolContactUser(contactId);

            return RedirectToAction("Index");
        }
    }
}