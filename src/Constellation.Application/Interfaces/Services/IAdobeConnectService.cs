using Constellation.Application.DTOs;
using Constellation.Core.Models;
using Constellation.Core.Models.Subjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IAdobeConnectService
    {
        Task<IEnumerable<AdobeConnectRoomDto>> UpdateRooms(string folderSco);
        Task<IEnumerable<AdobeConnectUserDetailDto>> UpdateUsers();
        Task<string> GetUserPrincipalId(string username);
        Task<ICollection<string>> GetSessionsForDate(string scoId, DateTime sessionDate);
        Task<ICollection<AdobeConnectSessionUserDetailDto>> GetSessionUserDetails(string scoId, string assetId);
        Task<string> CreateRoom(CourseOffering offering);
        Task<ServiceOperationResult<T>> ProcessOperation<T>(T operation) where T : AdobeConnectOperation;
        Task<ICollection<string>> GetCurrentSessionUsersAsync(string scoId, string assetId);
        Task<string> GetCurrentSessionAsync(string scoId);
    }
}
