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
using Core.Models.Faculties;
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
    
    public async Task<IActionResult> _GetGraphData(string id, int day)
    {
        var data = await _mediator.Send(new GetGraphDataForSchoolQuery { SchoolCode = id, Day = day });

        return Json(data);
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

        List<SchoolContact> contacts = await _contactRepository.GetWithRolesBySchool(id);
        List<string> roles = await _contactRepository.GetAvailableRoleList();

        // Build the master form viewmodel
        School_DetailsViewModel viewModel = new()
        {
            School = School_DetailsViewModel.SchoolDto.ConvertFromSchool(school)
        };

        foreach (SchoolContact contact in contacts)
        {
            List<SchoolContactRole> assignments = contact.Assignments
                .Where(role => 
                    !role.IsDeleted && 
                    role.SchoolCode == school.Code)
                .ToList();

            foreach (SchoolContactRole assignment in assignments)
                viewModel.Contacts.Add(School_DetailsViewModel.ContactDto.ConvertFromAssignment(contact, assignment));
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