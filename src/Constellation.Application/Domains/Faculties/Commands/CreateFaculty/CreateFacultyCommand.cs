namespace Constellation.Application.Domains.Faculties.Commands.CreateFaculty;

using Abstractions.Messaging;

public sealed record CreateFacultyCommand(
        string Name,
        string Colour)
    : ICommand;