using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Interfaces.Gateways;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Core.Models;
using Constellation.Presentation.Server.Areas.Partner.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.User)]
    public class SchoolsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISchoolService _schoolService;
        private readonly INetworkStatisticsGateway _budService;

        public SchoolsController(IUnitOfWork unitOfWork, ISchoolService schoolService,
            INetworkStatisticsGateway budService)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _schoolService = schoolService;
            _budService = budService;
        }

        public async Task<IActionResult> Index()
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
            var school = _unitOfWork.Schools.WithDetails(id);
            if (school == null)
                return null;

            var data = await _budService.GetSiteDetails(id);
            await _budService.GetSiteUsage(data, day);

            // What day is the graph representing?
            var dateDay = data.WANData.First().Time.Date;

            // What day of the cycle is this?
            var cyclicalDay = dateDay.GetDayNumber();

            var periods = new List<TimetablePeriod>();

            // Get the periods for all students at this school
            foreach (var student in school.Students)
            {
                periods.AddRange(_unitOfWork.Periods.AllForStudent(student.StudentId));
            }

            // Which of these periods are on the right day?
            periods = periods.Where(p => p.Day == cyclicalDay).ToList();

            var returnData = new School_GraphData
            {
                GraphDate = dateDay.ToString("D"),
                GraphSiteName = data.SiteName,
                GraphIntlDate = dateDay.ToString("yyyy-MM-dd")
            };

            foreach (var dataPoint in data.WANData)
            {
                var classSession = periods.Any(p => p.StartTime <= dataPoint.Time.TimeOfDay && p.EndTime >= dataPoint.Time.TimeOfDay);

                var point = new School_GraphDataPoint
                {
                    Time = dataPoint.Time.ToString("HH:mm"),
                    Lesson = classSession,
                    Networks = new List<School_GraphDataPointDetail>
                    {
                        new School_GraphDataPointDetail
                        {
                            Network = "WAN",
                            Connection = data.WANBandwidth.IfNotNull(c => c / 1000000),
                            Inbound = decimal.Round(dataPoint.WANInbound, 2),
                            Outbound = decimal.Round(dataPoint.WANOutbound, 2),
                        },
                        new School_GraphDataPointDetail
                        {
                            Network = "INT",
                            Connection = data.INTBandwidth.IfNotNull(c => c / 1000000),
                            Inbound = decimal.Round(dataPoint.INTInbound, 2),
                            Outbound = decimal.Round(dataPoint.INTOutbound, 2),
                        }
                    }
                };

                returnData.GraphData.Add(point);
            }

            return Json(returnData);
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
                //TODO: Convert services to async code
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
            viewModel.Students = school.Students.Where(student => !student.IsDeleted).Select(School_DetailsViewModel.StudentDto.ConvertFromStudent).ToList();
            viewModel.Staff = school.Staff.Where(staff => !staff.IsDeleted).Select(School_DetailsViewModel.StaffDto.ConvertFromStaff).ToList();

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
        [HttpPost]
        public async Task<IActionResult> ViewMap(IList<string> schoolCodes)
        {
            var vm = await CreateViewModel<School_MapViewModel>();

            vm.Layers = _unitOfWork.Schools.GetForMapping(schoolCodes);

            return View("Map", vm);
        }
    }
}