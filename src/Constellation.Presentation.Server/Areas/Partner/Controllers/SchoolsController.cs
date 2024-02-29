namespace Constellation.Presentation.Server.Areas.Partner.Controllers;

using Application.Faculties.GetFaculty;
using Constellation.Application.DTOs;
using Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;
using Constellation.Application.Features.API.Schools.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using Core.Models;
using Core.Models.Faculty;
using Core.Models.SchoolContacts;
using Core.Models.SchoolContacts.Repositories;
using Core.Models.Students;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

[Area("Partner")]
[Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
public class SchoolsController : Controller
{
    private readonly ISchoolContactRepository _contactRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISchoolService _schoolService;
    private readonly IMediator _mediator;

    public SchoolsController(
        ISchoolContactRepository contactRepository,
        IUnitOfWork unitOfWork,
        ISchoolService schoolService,
        IMediator mediator)
    {
        _contactRepository = contactRepository;
        _unitOfWork = unitOfWork;
        _schoolService = schoolService;
        _mediator = mediator;
    }

    public IActionResult Index()
    {
        return RedirectToAction("WithEither");
    }

    public async Task<IActionResult> All()
    {
        var schools = await _unitOfWork.Schools.ForListAsync(school => true);

        var viewModel = new School_ViewModel();
        viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

        return View("Index", viewModel);
    }

    public async Task<IActionResult> WithStudents()
    {
        var schools = await _unitOfWork.Schools.ForListAsync(school => school.Students.Any(student => !student.IsDeleted));

        var viewModel = new School_ViewModel();
        viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

        return View("Index", viewModel);
    }

    public async Task<IActionResult> WithStaff()
    {
        var schools = await _unitOfWork.Schools.ForListAsync(school => school.Staff.Any(staff => !staff.IsDeleted));

        var viewModel = new School_ViewModel();
        viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

        return View("Index", viewModel);
    }

    public async Task<IActionResult> WithBoth()
    {
        var schools = await _unitOfWork.Schools.ForListAsync(school => school.Students.Any(student => !student.IsDeleted) && school.Staff.Any(staff => !staff.IsDeleted));

        var viewModel = new School_ViewModel();
        viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

        return View("Index", viewModel);
    }

    public async Task<IActionResult> WithEither()
    {
        var schools = await _unitOfWork.Schools.ForListAsync(school => school.Students.Any(student => !student.IsDeleted) || school.Staff.Any(staff => !staff.IsDeleted));


        var viewModel = new School_ViewModel();
        viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

        return View("Index", viewModel);
    }

    public async Task<IActionResult> WithNeither()
    {
        var schools = await _unitOfWork.Schools.ForListAsync(school => !school.Students.Any(student => !student.IsDeleted) && !school.Staff.Any(staff => !staff.IsDeleted));

        var viewModel = new School_ViewModel();
        viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

        return View("Index", viewModel);
    }

    public async Task<IActionResult> _GetGraphData(string id, int day)
    {
        var data = await _mediator.Send(new GetGraphDataForSchoolQuery { SchoolCode = id, Day = day });

        return Json(data);
    }

    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    public async Task<IActionResult> Update(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToAction("Index");
        }

        var school = await _unitOfWork.Schools.ForEditAsync(id);

        if (school == null)
        {
            return RedirectToAction("Index");
        }

        var viewModel = new School_UpdateViewModel();
        viewModel.Resource = new SchoolDto
        {
            Code = school.Code,
            Name = school.Name,
            Address = school.Address,
            Division = school.Division,
            Electorate = school.Electorate,
            EmailAddress = school.EmailAddress,
            FaxNumber = school.FaxNumber,
            HeatSchool = school.HeatSchool,
            PhoneNumber = school.PhoneNumber,
            PostCode = school.PostCode,
            PrincipalNetwork = school.PrincipalNetwork,
            RollCallGroup = school.RollCallGroup,
            State = school.State,
            TimetableApplication = school.TimetableApplication,
            Town = school.Town
        };
            
        viewModel.IsNew = false;

        return View(viewModel);
    }

    [HttpPost]
    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(School_UpdateViewModel returnModel)
    {
        if (!ModelState.IsValid)
        {
            return View("Update", returnModel);
        }

        if (returnModel.IsNew)
        {
            await _schoolService.CreateSchool(returnModel.Resource);
        }
        else
        {
            await _schoolService.UpdateSchool(returnModel.Resource.Code, returnModel.Resource);
        }

        await _unitOfWork.CompleteAsync();

        return RedirectToAction("Index");
    }

    [Roles(AuthRoles.Admin, AuthRoles.Editor)]
    public async Task<IActionResult> Create()
    {
        var viewModel = new School_UpdateViewModel();
        viewModel.Resource = new SchoolDto();
        viewModel.IsNew = true;

        return View("Update", viewModel);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return RedirectToAction("Index");
        }

        School school = await _unitOfWork.Schools.ForDetailDisplayAsync(id);

        if (school == null)
        {
            return RedirectToAction("Index");
        }

        List<SchoolContact> contacts = await _contactRepository.GetAll();
        List<string> roles = await _contactRepository.GetAvailableRoleList();

        // Build the master form viewmodel
        School_DetailsViewModel viewModel = new School_DetailsViewModel();
        viewModel.School = School_DetailsViewModel.SchoolDto.ConvertFromSchool(school);

        foreach (SchoolContact contact in contacts.Where(contact => contact.Assignments.Any(role => !role.IsDeleted && role.SchoolCode == school.Code)))
        {
            foreach (SchoolContactRole assignment in contact.Assignments.Where(role => !role.IsDeleted && role.SchoolCode == school.Code).ToList())
            {
                viewModel.Contacts.Add(School_DetailsViewModel.ContactDto.ConvertFromAssignment(contact, assignment));
            }
        }
            
        foreach (Student student in school.Students)
        {
            if (student.IsDeleted)
                continue;

            Result<List<StudentEnrolmentResponse>> enrolments = await _mediator.Send(new GetStudentEnrolmentsWithDetailsQuery(student.StudentId));

            viewModel.Students.Add(new()
            {
                StudentId = student.StudentId,
                Gender = student.Gender,
                Name = student.DisplayName,
                Grade = student.CurrentGrade,
                Enrolments = enrolments
                    .Value
                    .Select(enrolment => enrolment.OfferingName)
                    .OrderBy(entry => entry)
                    .ToList()
            });
        }
            
        foreach (Staff member in school.Staff.Where(staff => !staff.IsDeleted))
        {
            List<string> faculties = new();

            foreach (FacultyMembership membership in member.Faculties.Where(membership => !membership.IsDeleted))
            {
                Result<FacultyResponse> facultyRequest = await _mediator.Send(new GetFacultyQuery(membership.FacultyId));

                if (facultyRequest.IsSuccess)
                {
                    faculties.Add(facultyRequest.Value.Name);
                }
            }

            Result<List<TeacherOfferingResponse>> courseList = await _mediator.Send(new GetCurrentOfferingsForTeacherQuery(member.StaffId));

            School_DetailsViewModel.StaffDto entry = new School_DetailsViewModel.StaffDto
            {
                Id = member.StaffId,
                Name = member.DisplayName,
                Faculty = faculties,
                Courses = courseList
                    .Value
                    .Select(course => course.OfferingName.Value)
                    .OrderBy(entry => entry)
                    .ToList() 
            };

            viewModel.Staff.Add(entry);
        }                

        viewModel.RoleAssignmentDto = new Contacts_AssignmentViewModel
        {
            ContactRole = new SchoolContactRoleDto
            {
                SchoolCode = school.Code,
                SchoolName = school.Name
            },
            SchoolStaffList = new SelectList(contacts, "Id", "DisplayName"),
            RoleList = new SelectList(roles)
        };

        return View(viewModel);
    }

    [Authorize]
    [HttpPost("/Partner/Schools/Map")]
    public async Task<IActionResult> ViewMap(IList<string> schoolCodes)
    {
        var vm = new School_MapViewModel();

        vm.Layers = _unitOfWork.Schools.GetForMapping(schoolCodes);
        vm.PageHeading = "Map of Schools";

        return View("Map", vm);
    }

    [AllowAnonymous]
    [HttpGet("/Partner/Schools/Map")]
    public async Task<IActionResult> ViewAnonMap()
    {
        School_MapViewModel vm = new School_MapViewModel();

        vm.Layers = _unitOfWork.Schools.GetForMapping(new List<string>());
        vm.PageHeading = "Map of Schools";

        return View("Map", vm);
    }
}