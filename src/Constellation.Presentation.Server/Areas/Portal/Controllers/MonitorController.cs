using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.Portal.Controllers
{
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

        public async Task<IActionResult> StatusPopup(int id)
        {
            var statusList = await _classMonitorCacheService.GetCurrentStatus();
            var course = statusList.FirstOrDefault(status => status.Id == id) ?? new ClassMonitorDtos.MonitorCourse();

            return PartialView("StatusPopup", course);
        }
    }
}
