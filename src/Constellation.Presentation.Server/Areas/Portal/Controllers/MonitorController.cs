using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Controllers
{
    using Application.Models.Auth;
    using Microsoft.AspNetCore.Authorization;

    [Authorize(Policy = AuthPolicies.IsStaffMember)]
    [Area("Portal")]
    public class MonitorController : Controller
    {
        private readonly IClassMonitorCacheService _classMonitorCacheService;

        public MonitorController(IClassMonitorCacheService classMonitorCacheService)
        {
            _classMonitorCacheService = classMonitorCacheService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Dashboard()
        {
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DashboardStatus()
        {
            var statusList = await _classMonitorCacheService.GetCurrentStatus();

            return PartialView(statusList);
        }

        public async Task<IActionResult> StatusPopup(Guid id)
        {
            var statusList = await _classMonitorCacheService.GetCurrentStatus();
            var course = statusList.FirstOrDefault(status => status.Id.Value == id) ?? new ClassMonitorDtos.MonitorCourse();

            return PartialView("StatusPopup", course);
        }
    }
}
