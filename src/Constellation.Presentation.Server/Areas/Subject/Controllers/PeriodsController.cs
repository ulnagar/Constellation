using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Auth;
using Constellation.Presentation.Server.Areas.Subject.Models;
using Constellation.Presentation.Server.BaseModels;
using Constellation.Presentation.Server.Helpers.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Subject.Controllers
{
    [Area("Subject")]
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.StaffMember)]
    public class PeriodsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;

        public PeriodsController(IUnitOfWork unitOfWork, ISessionService sessionService)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await CreateViewModel<Periods_ViewModel>();
            viewModel.Periods = await _unitOfWork.Periods.ForGraphicalDisplayAsync();

            return View(viewModel);
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> Create()
        {
            var periods = await _unitOfWork.Periods.ForSelectionAsync();
            var timetables = periods.Select(period => period.Timetable).Distinct().ToList();
            var types = periods.Select(period => period.Type).Distinct().ToList();

            var viewModel = await CreateViewModel<Periods_UpdateViewModel>();
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

            var period = _unitOfWork.Periods.WithDetails(id);

            if (period == null)
            {
                return RedirectToAction("Index");
            }

            var periods = await _unitOfWork.Periods.ForSelectionAsync();
            var timetables = periods.Select(period => period.Timetable).Distinct().ToList();
            var types = periods.Select(period => period.Type).Distinct().ToList();

            var viewModel = await CreateViewModel<Periods_UpdateViewModel>();
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
                await UpdateViewModel(viewModel);
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