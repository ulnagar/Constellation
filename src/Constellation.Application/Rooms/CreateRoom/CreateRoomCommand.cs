namespace Constellation.Application.Rooms.CreateRoom;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Rooms.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Models.Subjects.Identifiers;

public sealed record CreateRoomCommand(
    OfferingId OfferingId)
    : ICommand<RoomResponse>;