namespace Constellation.Application.Faculties.UpdateFaculty;

using Abstractions.Messaging;
using Core.Models.Faculty.Identifiers;

public sealed record UpdateFacultyCommand(
    FacultyId Id,
    string Name,
    string Colour) 
    : ICommand;