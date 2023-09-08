namespace Constellation.Application.Rooms.GetRoomById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Interfaces.Repositories;
using Constellation.Application.Rooms.Models;
using Constellation.Core.Errors;
using Constellation.Core.Models;
using Constellation.Core.Shared;
using Serilog;
using System.Threading;
using System.Threading.Tasks;

internal sealed class GetRoomByIdQueryHandler
    : IQueryHandler<GetRoomByIdQuery, RoomResponse>
{
    private readonly IAdobeConnectRoomRepository _roomRepository;
    private readonly ILogger _logger;

    public GetRoomByIdQueryHandler(
        IAdobeConnectRoomRepository roomRepository,
        ILogger logger)
    {
        _roomRepository = roomRepository;
        _logger = logger.ForContext<GetRoomByIdQuery>();
    }

    public async Task<Result<RoomResponse>> Handle(GetRoomByIdQuery request, CancellationToken cancellationToken)
    {
        AdobeConnectRoom room = await _roomRepository.GetById(request.ScoId, cancellationToken);

        if (room is null)
        {
            _logger
                .ForContext(nameof(GetRoomByIdQuery), request, true)
                .ForContext(nameof(Error), DomainErrors.LinkedSystems.AdobeConnect.RoomNotFound(request.ScoId), true)
                .Warning("Failed to retrieve Adobe Connect Room");

            return Result.Failure<RoomResponse>(DomainErrors.LinkedSystems.AdobeConnect.RoomNotFound(request.ScoId));
        }

        return new RoomResponse(
            room.ScoId,
            room.Name,
            room.UrlPath,
            room.IsDeleted);
    }
}
