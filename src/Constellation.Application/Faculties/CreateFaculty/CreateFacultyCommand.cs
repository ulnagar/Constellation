namespace Constellation.Application.Faculties.CreateFaculty;

using Abstractions.Messaging;

public sealed record CreateFacultyCommand(
        string Name,
        string Colour)
    : ICommand;