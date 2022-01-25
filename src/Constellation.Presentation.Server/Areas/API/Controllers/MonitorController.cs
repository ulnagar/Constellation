using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Presentation.Server.Areas.API.Controllers
{
    [Route("api/v1/Monitor")]
    [ApiController]
    public class MonitorController : ControllerBase
    {
        private readonly IAdobeConnectService _adobeConnectService;
        private readonly IClassMonitorCacheService _monitorCacheService;

        public MonitorController(IAdobeConnectService adobeConnectService, IClassMonitorCacheService monitorCacheService)
        {
            _adobeConnectService = adobeConnectService;
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
            return await _adobeConnectService.GetCurrentSessionAsync(scoId);
        }

        [HttpGet("GetRoomUsers")]
        public async Task<ICollection<string>> GetRoomUsers(string scoId, string assetId)
        {
            return await _adobeConnectService.GetCurrentSessionUsersAsync(scoId, assetId);
        }
    }
}