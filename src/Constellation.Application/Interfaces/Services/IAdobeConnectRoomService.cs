using Constellation.Application.DTOs;
using Constellation.Core.Models;
using System.Threading.Tasks;

namespace Constellation.Application.Interfaces.Services
{
    public interface IAdobeConnectRoomService
    {
        Task<ServiceOperationResult<AdobeConnectRoom>> CreateRoom(AdobeConnectRoomDto roomResource);
        Task<ServiceOperationResult<AdobeConnectRoom>> UpdateRoom(string id, AdobeConnectRoomDto roomResource);
        Task RemoveRoom(string id);
        Task ProtectRoom(string id);
    }
}
