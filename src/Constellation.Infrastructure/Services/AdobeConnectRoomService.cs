using Constellation.Application.DTOs;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Interfaces.Services;
using Constellation.Core.Models;
using Constellation.Infrastructure.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Constellation.Infrastructure.Services
{
    // Reviewed for ASYNC Operations
    public class AdobeConnectRoomService : IAdobeConnectRoomService, IScopedService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdobeConnectRoomService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceOperationResult<AdobeConnectRoom>> CreateRoom(AdobeConnectRoomDto roomResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<AdobeConnectRoom>();

            // Validate entry
            var checkRoom = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomResource.ScoId);

            if (checkRoom == null)
            {
                var room = new AdobeConnectRoom
                {
                    ScoId = roomResource.ScoId,
                    Name = roomResource.Name,
                    UrlPath = roomResource.UrlPath,
                    Protected = roomResource.Protected
                };

                _unitOfWork.Add(room);

                result.Success = true;
                result.Entity = room;
            }
            else
            {
                result.Success = false;
                result.Errors.Add($"A room with that ID already exists!");
            }

            return result;
        }

        public async Task<ServiceOperationResult<AdobeConnectRoom>> UpdateRoom(string id, AdobeConnectRoomDto roomResource)
        {
            // Set up return object
            var result = new ServiceOperationResult<AdobeConnectRoom>();

            // Validate entry
            var room = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(roomResource.ScoId);

            if (room != null)
            {
                if (!string.IsNullOrWhiteSpace(roomResource.ScoId))
                    room.ScoId = roomResource.ScoId;

                if (!string.IsNullOrWhiteSpace(roomResource.Name))
                    room.Name = roomResource.Name;

                if (!string.IsNullOrWhiteSpace(roomResource.UrlPath))
                    room.UrlPath = roomResource.UrlPath;

                result.Success = true;
                result.Entity = room;
            }
            else
            {
                result.Success = false;
                result.Errors.Add($"A room with that ID already exists!");
            }

            return result;
        }

        public async Task RemoveRoom(string id)
        {
            // Validate entries
            var room = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(id);

            if (room == null)
                return;

            room.IsDeleted = true;
            room.DateDeleted = DateTime.Now;
        }

        public async Task ProtectRoom(string id)
        {
            // Validate entries
            var room = await _unitOfWork.AdobeConnectRooms.GetForExistCheck(id);

            if (room == null)
                return;

            room.Protected = true;
        }
    }
}
