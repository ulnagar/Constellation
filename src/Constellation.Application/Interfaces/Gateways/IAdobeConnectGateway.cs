using Constellation.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Gateways
{
    public interface IAdobeConnectGateway
    {
        Task<string> GetUserPrincipalId(string username);
        Task<bool> UserPermissionUpdate(string principalId, string scoId, string accessLevel);
        Task<bool> GroupMembershipUpdate(string principalId, string groupId, string accessLevel);
        Task<ICollection<string>> GetSessionsForDate(string scoId, DateTime sessionDate);
        Task<ICollection<string>> GetSessionUsers(string scoId, string assetId);
        Task<ICollection<AdobeConnectSessionUserDetailDto>> GetSessionUserDetails(string scoId, string assetId);
        Task<AdobeConnectRoomDto> CreateRoom(string name, string dateStart, string dateEnd, string urlPath, bool detectParentFolder, string parentFolder, string year, string grade, bool useTemplate, string faculty);
        Task<ICollection<AdobeConnectRoomDto>> ListRooms(string folderSco);
        Task<string> GetCurrentSession(string scoId);
    }
}
