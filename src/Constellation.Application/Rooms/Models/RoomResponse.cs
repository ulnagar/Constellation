namespace Constellation.Application.Rooms.Models;

public sealed record RoomResponse(
    string ScoId,
    string Name,
    string UrlPath,
    bool IsDeleted);