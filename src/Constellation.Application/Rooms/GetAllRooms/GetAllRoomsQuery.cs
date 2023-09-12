namespace Constellation.Application.Rooms.GetAllRooms;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Rooms.Models;
using System.Collections.Generic;

public sealed record GetAllRoomsQuery()
    : IQuery<List<RoomResponse>>;