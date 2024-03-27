namespace Constellation.Application.Rooms.CreateRoom;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Rooms.Models;
using Constellation.Core.Models.Offerings.Identifiers;

public sealed record CreateRoomCommand(
    OfferingId OfferingId)
    : ICommand<RoomResponse>;