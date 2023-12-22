using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.Areas.Subject.Models;
using Constellation.Presentation.Server.Helpers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Constellation.Presentation.Server.Areas.Subject.Controllers
{
    [Area("Subject")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class PeriodsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPeriodService _sessionService;

        public PeriodsController(
            IUnitOfWork unitOfWork, 
            IPeriodService sessionService,
            IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new Periods_ViewModel();
            viewModel.Periods = await _unitOfWork.Periods.ForGraphicalDisplayAsync();

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Create()
        {
            var periods = await _unitOfWork.Periods.ForSelectionAsync();
            var timetables = periods.Select(period => period.Timetable).Distinct().ToList();
            var types = periods.Select(period => period.Type).Distinct().ToList();

            var viewModel = new Periods_UpdateViewModel();
            viewModel.IsNew = true;
            viewModel.TimetableList = new SelectList(timetables);
            viewModel.TypeList = new SelectList(types);

            return View("Update", viewModel);
        }


        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Update(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index");
            }

            var period = await _unitOfWork.Periods.GetById(id);

            if (period == null)
            {
                return RedirectToAction("Index");
            }

            var periods = await _unitOfWork.Periods.ForSelectionAsync();
            var timetables = periods.Select(period => period.Timetable).Distinct().ToList();
            var types = periods.Select(period => period.Type).Distinct().ToList();

            var viewModel = new Periods_UpdateViewModel();
            viewModel.Period = new PeriodDto
            {
                Id = period.Id,
                Day = period.Day,
                Name = period.Name,
                StartTime = period.StartTime,
                EndTime = period.EndTime,
                Period = period.Period,
                Timetable = period.Timetable,
                Type = period.Type
            };
            viewModel.TimetableList = new SelectList(timetables);
            viewModel.TypeList = new SelectList(types);

            return View(viewModel);
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Periods_UpdateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var periods = await _unitOfWork.Periods.ForSelectionAsync();
                var timetables = periods.Select(period => period.Timetable).Distinct().ToList();
                var types = periods.Select(period => period.Type).Distinct().ToList();
                viewModel.TimetableList = new SelectList(timetables);
                viewModel.TypeList = new SelectList(types);

                return View(viewModel);
            }

            if (viewModel.IsNew)
            {
                await _sessionService.CreatePeriod(viewModel.Period);
            }
            else
            {
                await _sessionService.UpdatePeriod(viewModel.Period.Id, viewModel.Period);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Index");
        }
    }
}