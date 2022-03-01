using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Application.Models.Identity;
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
    [Roles(AuthRoles.Admin, AuthRoles.Editor, AuthRoles.User)]
    public class SessionsController : BaseController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISessionService _sessionService;

        public SessionsController(IUnitOfWork unitOfWork, ISessionService sessionService)
            : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _sessionService = sessionService;
        }

        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> _AddSession(Sessions_AssignmentViewModel viewModel)
        {
            var check = await _unitOfWork.OfferingSessions.ForOfferingAndPeriod(viewModel.OfferingId, viewModel.PeriodId);
            if (check.Count == 0)
            {
                var assignment = new SessionDto
                {
                    OfferingId = viewModel.OfferingId,
                    PeriodId = viewModel.PeriodId,
                    StaffId = viewModel.TeacherId,
                    RoomId = viewModel.RoomId
                };

                await _sessionService.CreateSession(assignment);

                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction("Details", "Classes", new { area = "Subject", id = viewModel.OfferingId });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> AddBulkSession(int id)
        {
            var viewModel = await CreateViewModel<Sessions_BulkAssignmentViewModel>();

            if (id == 0)
            {
                return null;
            }
            var offering = await _unitOfWork.CourseOfferings.ForSessionEditAsync(id);

            if (offering == null)
            {
                return null;
            }

            var staff = await _unitOfWork.Staff.ForSelectionAsync();
            var rooms = await _unitOfWork.AdobeConnectRooms.ForSelectionAsync();

            viewModel.OfferingId = id;
            viewModel.OfferingName = offering.Name;
            viewModel.Periods = offering.Sessions.Where(s => !s.IsDeleted).Select(s => s.PeriodId).ToList();
            viewModel.ValidPeriods = await _unitOfWork.Periods.ForSelectionAsync();
            viewModel.StaffList = new SelectList(staff, "StaffId", "DisplayName");
            viewModel.RoomList = new SelectList(rooms, "ScoId", "Name");

            return View("AddSession", viewModel);
        }


        [HttpPost]
        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBulkSession(Sessions_BulkAssignmentViewModel viewModel)
        {
            var offering = _unitOfWork.CourseOfferings.WithDetails(viewModel.OfferingId);
            var teacher = _unitOfWork.Staff.WithDetails(viewModel.TeacherId);
            var room = _unitOfWork.AdobeConnectRooms.WithDetails(viewModel.RoomId);

            foreach (var periodId in viewModel.Periods)
            {
                var period = await _unitOfWork.Periods.ForEditAsync(periodId);

                // Is this class already scheduled during this period?
                var check = await _unitOfWork.OfferingSessions.ForOfferingAndPeriod(offering.Id, period.Id);
                if (check.Count > 0)
                    continue;

                var assignment = new SessionDto
                {
                    OfferingId = offering.Id,
                    PeriodId = period.Id,
                    StaffId = teacher.StaffId,
                    RoomId = room.ScoId
                };

                // TODO: Convert service to async code
                await _sessionService.CreateSession(assignment);

                await _unitOfWork.CompleteAsync();
            }

            return RedirectToAction("Details", "Classes", new { area = "Subject", id = offering.Name, code = offering.Id });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> RemoveSession(int id)
        {
            if (id == 0)
            {
                return RedirectToAction("Index", "Classes", new { area = "Subject" });
            }

            var session = await _unitOfWork.OfferingSessions.ForExistCheckAsync(id);

            if (session == null)
            {
                return RedirectToAction("Index", "Classes", new { area = "Subject" });
            }

            // TODO: Convert service to async code
            await _sessionService.RemoveSession(id);
            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", "Classes", new { area = "Subject", id = session.OfferingId });
        }

        [Roles(AuthRoles.Admin, AuthRoles.Editor)]
        public async Task<IActionResult> BulkRemoveSession(int offeringId)
        {
            if (offeringId == 0)
            {
                return RedirectToAction("Index", "Classes", new { area = "Subject" });
            }

            var offering = await _unitOfWork.CourseOfferings.ForSessionEditAsync(offeringId);

            if (offering == null)
            {
                return RedirectToAction("Index", "Classes");
            }

            foreach (var session in offering.Sessions.Where(s => !s.IsDeleted))
            {
                await _sessionService.RemoveSession(session.Id);
            }

            await _unitOfWork.CompleteAsync();

            return RedirectToAction("Details", "Classes", new { area = "Subject", id = offering.Id });
        }
    }
}