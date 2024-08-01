using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Constellation.Presentation.Server.Areas.API.Controllers
{
    [Route("api/v1/Monitor")]
    [ApiController]
    public class MonitorController : ControllerBase
    {
        private readonly IClassMonitorCacheService _monitorCacheService;

        public MonitorController(IClassMonitorCacheService monitorCacheService)
        {
            _monitorCacheService = monitorCacheService;
        }

        [HttpGet("GetData")]
        public async Task<ClassMonitorDtos> GetData()
        {
            return await _monitorCacheService.GetData();
        }

        [HttpGet("GetRoomSession")]
        public async Task<string> GetRoomSession(string scoId)
        {
            return string.Empty;
        }

        [HttpGet("GetRoomUsers")]
        public async Task<ICollection<string>> GetRoomUsers(string scoId, string assetId)
        {
            return new List<string>();
        }
    }
}