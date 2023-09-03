namespace Constellation.Application.Rooms.GetAllRooms;

public sealed record RoomResponse(
    string ScoId,
    string Name,
    string UrlPath,
    bool IsDeleted);