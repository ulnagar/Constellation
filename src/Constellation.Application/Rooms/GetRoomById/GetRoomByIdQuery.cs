namespace Constellation.Application.Rooms.GetRoomById;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Rooms.Models;

public sealed record GetRoomByIdQuery(
    string ScoId)
    : IQuery<RoomResponse>;
