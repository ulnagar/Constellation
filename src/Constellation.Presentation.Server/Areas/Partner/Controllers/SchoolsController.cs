﻿using Constellation.Application.DTOs;
using Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;
using Constellation.Application.Features.API.Schools.Queries;
using Constellation.Application.Features.Faculties.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Application.Offerings.GetCurrentOfferingsForTeacher;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class SchoolsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISchoolService _schoolService;
        private readonly IMediator _mediator;

        public SchoolsController(
            IUnitOfWork unitOfWork, 
            ISchoolService schoolService,
            IMediator mediator)
            : base(mediator)
        {
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

            var viewModel = await CreateViewModel<School_ViewModel>();
            viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithStudents()
        {
            var schools = await _unitOfWork.Schools.ForListAsync(school => school.Students.Any(student => !student.IsDeleted));

            var viewModel = await CreateViewModel<School_ViewModel>();
            viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithStaff()
        {
            var schools = await _unitOfWork.Schools.ForListAsync(school => school.Staff.Any(staff => !staff.IsDeleted));

            var viewModel = await CreateViewModel<School_ViewModel>();
            viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithBoth()
        {
            var schools = await _unitOfWork.Schools.ForListAsync(school => school.Students.Any(student => !student.IsDeleted) && school.Staff.Any(staff => !staff.IsDeleted));

            var viewModel = await CreateViewModel<School_ViewModel>();
            viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithEither()
        {
            var schools = await _unitOfWork.Schools.ForListAsync(school => school.Students.Any(student => !student.IsDeleted) || school.Staff.Any(staff => !staff.IsDeleted));


            var viewModel = await CreateViewModel<School_ViewModel>();
            viewModel.Schools = schools.Select(School_ViewModel.SchoolDto.ConvertFromSchool).ToList();

            return View("Index", viewModel);
        }

        public async Task<IActionResult> WithNeither()
        {
            var schools = await _unitOfWork.Schools.ForListAsync(school => !school.Students.Any(student => !student.IsDeleted) && !school.Staff.Any(staff => !staff.IsDeleted));

            var viewModel = await CreateViewModel<School_ViewModel>();
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

            var viewModel = await CreateViewModel<School_UpdateViewModel>();
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
                await UpdateViewModel(returnModel);
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
            var viewModel = await CreateViewModel<School_UpdateViewModel>();
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

            var school = await _unitOfWork.Schools.ForDetailDisplayAsync(id);

            if (school == null)
            {
                return RedirectToAction("Index");
            }

            var contacts = await _unitOfWork.SchoolContacts.ForSelectionAsync();
            var roles = await _unitOfWork.SchoolContactRoles.ListOfRolesForSelectionAsync();

            // Build the master form viewmodel
            var viewModel = await CreateViewModel<School_DetailsViewModel>();
            viewModel.School = School_DetailsViewModel.SchoolDto.ConvertFromSchool(school);
            viewModel.Contacts = school.StaffAssignments.Where(role => !role.IsDeleted).Select(School_DetailsViewModel.ContactDto.ConvertFromAssignment).ToList();
            
            foreach (var student in school.Students)
            {
                if (student.IsDeleted)
                    continue;

                var enrolments = await _mediator.Send(new GetStudentEnrolmentsWithDetailsQuery(student.StudentId));

                viewModel.Students.Add(new()
                {
                    StudentId = student.StudentId,
                    Gender = student.Gender,
                    Name = student.DisplayName,
                    Grade = student.CurrentGrade,
                    Enrolments = enrolments.Value.Select(enrolment => enrolment.OfferingName).ToList()
                });
            }
            
            foreach (var member in school.Staff.Where(staff => !staff.IsDeleted))
            {
                List<string> faculties = new();

                foreach (var membership in member.Faculties.Where(membership => !membership.IsDeleted))
                {
                    var faculty = await _mediator.Send(new GetFacultyNameQuery(membership.FacultyId));

                    if (!string.IsNullOrWhiteSpace(faculty))
                    {
                        faculties.Add(faculty);
                    }
                }

                var courseList = await _mediator.Send(new GetCurrentOfferingsForTeacherQuery(member.StaffId));

                var entry = new School_DetailsViewModel.StaffDto
                {
                    Id = member.StaffId,
                    Name = member.DisplayName,
                    Faculty = faculties,
                    Courses = courseList.Value.Select(course => course.OfferingName.Value).ToList() 
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
            var vm = await CreateViewModel<School_MapViewModel>();

            vm.Layers = _unitOfWork.Schools.GetForMapping(schoolCodes);
            vm.PageHeading = "Map of Schools";

            return View("Map", vm);
        }
    }
}