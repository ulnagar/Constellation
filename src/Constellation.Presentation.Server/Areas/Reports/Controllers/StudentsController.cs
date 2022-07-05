using Constellation.Application.DTOs;
using Constellation.Application.Extensions;
using Constellation.Application.Features.Awards.Queries;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
using Constellation.Presentation.Server.Areas.Reports.Models;
using Constellation.Presentation.Server.Areas.Reports.Models.Students;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ACOS.Web.Areas.Reports.Controllers
{
    [Area("Reports")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.User)]
    public class StudentsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAdobeConnectService _adobeConnectService;
        private readonly IMediator _mediator;

        public StudentsController(IUnitOfWork unitOfWork, IAdobeConnectService adobeConnectService, IMediator mediator)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _adobeConnectService = adobeConnectService;
            _mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await CreateViewModel<BaseViewModel>();

            return View(viewModel);
        }

        public async Task<IActionResult> FTEBreakdown()
        {
            var viewModel = await CreateViewModel<Student_FTEBreakdown_ViewModel>();

            var students = await _unitOfWork.Students.AllActiveForFTECalculations();
            var byGrade = students.GroupBy(student => student.CurrentGrade);

            foreach (var grade in byGrade)
            {
                var gradeEntry = new Student_FTEBreakdown_ViewModel.GradeEntry
                {
                    Grade = $"Year {grade.Key.AsNumber().PadLeft(2, '0')}",
                    MaleEnrolments = grade.Count(student => student.Gender == "M"),
                    FemaleEnrolments = grade.Count(student => student.Gender == "F")
                };

                var enrolments = grade.SelectMany(student => student.Enrolments.Where(enrol => !enrol.IsDeleted));

                foreach (var enrolment in enrolments)
                {
                    if (enrolment.Student.Gender == "M")
                        gradeEntry.MaleEnrolmentFTE += enrolment.Offering.Course.FullTimeEquivalentValue;

                    if (enrolment.Student.Gender == "F")
                        gradeEntry.FemaleEnrolmentFTE += enrolment.Offering.Course.FullTimeEquivalentValue;
                }

                viewModel.Grades.Add(gradeEntry);
            }

            viewModel.TotalMaleEnrolments = viewModel.Grades.Sum(grade => grade.MaleEnrolments);
            viewModel.TotalMaleEnrolmentFTE = viewModel.Grades.Sum(grade => grade.MaleEnrolmentFTE);
            viewModel.TotalFemaleEnrolments = viewModel.Grades.Sum(grade => grade.FemaleEnrolments);
            viewModel.TotalFemaleEnrolmentFTE = viewModel.Grades.Sum(grade => grade.FemaleEnrolmentFTE);

            viewModel.TotalEnrolments = viewModel.Grades.Sum(grade => grade.TotalEnrolments);
            viewModel.TotalEnrolmentFTE = viewModel.Grades.Sum(grade => grade.TotalEnrolmentFTE);

            return View(viewModel);
        }

        public async Task<IActionResult> AttendanceQuerySelection()
        {
            var viewModel = await CreateViewModel<Student_AttendanceQuerySelectionViewModel>();
            var students = await _unitOfWork.Students.ForSelectionListAsync();
            viewModel.StudentList = new SelectList(students, "StudentId", "DisplayName", null, "CurrentGrade");

            return View("AttendanceQuerySelection", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AttendanceQuerySelection(string studentId, DateTime lookupDate)
        {
            var viewModel = await CreateViewModel<Student_AttendanceQuerySelectionViewModel>();
            viewModel.StudentId = studentId;
            viewModel.LookupDate = lookupDate;

            var students = await _unitOfWork.Students.ForSelectionListAsync();
            viewModel.StudentList = new SelectList(students, "StudentId", "DisplayName", studentId);

            var student = await _unitOfWork.Students.ForAttendanceQueryReport(studentId);

            var selectedDay = lookupDate.GetDayNumber();

            var sessions = student.Enrolments.Where(e => !e.IsDeleted)
                .SelectMany(e => e.Offering.Sessions.Where(s => s.Period.Day == selectedDay));
            
            var dayPeriods = _unitOfWork.Periods.AllFromDay(selectedDay);

            viewModel.Periods = dayPeriods;

            foreach (var session in sessions)
            {
                var entry = new Student_AttendanceQuerySelection_ClassViewModel
                {
                    ClassId = session.OfferingId,
                    ClassName = session.Offering.Name,
                    PeriodId = session.PeriodId,
                    PeriodName = session.Period.Name,
                    RoomSco = session.RoomId
                };

                viewModel.ClassList.Add(entry);
            }

            return View("AttendanceQuerySelection", viewModel);
        }

        public async Task<IActionResult> AttendanceQuery(string studentId, DateTime lookupDate, string scoId)
        {
            var viewModel = await CreateViewModel<Student_AttendanceQueryViewModel>();

            var student = await _unitOfWork.Students.ForEditAsync(studentId);

            var sessions = await _adobeConnectService.GetSessionsForDate(scoId, lookupDate);
            foreach (var session in sessions)
            {
                var users = await _adobeConnectService.GetSessionUserDetails(scoId, session);
                foreach (var user in users)
                {
                    var entry = new Student_AttendanceQuery_UserListViewModel
                    {
                        Name = user.Name,
                        Login = user.Login,
                        LoginTime = user.LoginTime,
                        LogoutTime = user.LogoutTime
                    };

                    entry.Highlighted = (string.Equals(entry.Login, (student.PortalUsername + "@detnsw"), StringComparison.CurrentCultureIgnoreCase));

                    viewModel.Users.Add(entry);
                }
            }

            return View("AttendanceQuery", viewModel);
        }

        public async Task<IActionResult> AbsenceSettings()
        {
            var students = await _unitOfWork.Students.AllWithAbsenceScanSettings();

            var viewModel = await CreateViewModel<Student_AbsenceSettings_ViewModel>();

            foreach (var student in students)
            {
                var item = new Student_AbsenceSettings_ViewModel.StudentItem
                {
                    Id = student.StudentId,
                    Name = student.DisplayName,
                    SchoolName = student.School.Name,
                    Grade = student.CurrentGrade,
                    IsEnabled = student.IncludeInAbsenceNotifications,
                    EnabledFromDate = student.AbsenceNotificationStartDate,
                };

                viewModel.Students.Add(item);
            }

            viewModel.Students = viewModel.Students.OrderBy(student => student.Name).OrderBy(student => student.SchoolName).ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> InterviewDetails()
        {
            var offerings = await _unitOfWork.CourseOfferings.ForSelectionAsync();

            var viewModel = await CreateViewModel<Student_InterviewDetails_ViewModel>();
            viewModel.AllClasses = new SelectList(offerings, "Id", "Name");

            return View(viewModel);
        }

        public async Task<IActionResult> Awards()
        {
            var viewModel = await CreateViewModel<AwardsListViewModel>();

            var students = await _mediator.Send(new GetStudentsWithAwardQuery());

            foreach (var student in students.OrderBy(student => student.CurrentGrade).ThenBy(student => student.LastName))
            {
                var entry = new AwardsListViewModel.AwardRecord();

                entry.StudentId = student.StudentId;
                entry.StudentName = student.DisplayName;
                entry.StudentGrade = student.CurrentGrade.AsName();

                entry.AwardedAstras = student.Awards.Count(award => award.Type == "Astra Award");
                entry.AwardedStellars = student.Awards.Count(award => award.Type == "Stellar Award");
                entry.AwardedGalaxies = student.Awards.Count(award => award.Type == "Galaxy Medal");
                entry.AwardedUniversals = student.Awards.Count(award => award.Type == "Aurora Universal Achiever");

                entry.CalculatedStellars = Math.Floor(entry.AwardedAstras / 5);
                entry.CalculatedGalaxies = Math.Floor(entry.AwardedAstras / 25);
                entry.CalculatedUniversals = Math.Floor(entry.AwardedAstras / 125);

                if (entry.AwardedStellars != entry.CalculatedStellars ||
                    entry.AwardedGalaxies != entry.CalculatedGalaxies ||
                    entry.AwardedUniversals != entry.CalculatedUniversals)
                    viewModel.Awards.Add(entry);
            }

            return View(viewModel);
        }
    }
}