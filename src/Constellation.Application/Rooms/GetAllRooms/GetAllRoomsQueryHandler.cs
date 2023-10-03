namespace Constellation.Application.Rooms.GetAllRooms;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Rooms.Models;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetAllRoomsQueryHandler
    : IQueryHandler<GetAllRoomsQuery, List<RoomResponse>>
{
    private readonly IAdobeConnectRoomRepository _roomRepository;
    private readonly ILogger _logger;

    public GetAllRoomsQueryHandler(
        IAdobeConnectRoomRepository roomRepository,
        ILogger logger)
    {
        _roomRepository = roomRepository;
        _logger = logger.ForContext<GetAllRoomsQuery>();
    }

    public async Task<Result<List<RoomResponse>>> Handle(GetAllRoomsQuery request, CancellationToken cancellationToken)
    {
        List<AdobeConnectRoom> rooms = await _roomRepository.GetAll(cancellationToken);

        List<RoomResponse> response = new();

        foreach (AdobeConnectRoom room in rooms)
        {
            response.Add(new(
                room.ScoId,
                room.Name,
                room.UrlPath,
                room.IsDeleted));
        }

        return response;
    }
}
