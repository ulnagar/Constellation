namespace Constellation.Application.Domains.Faculties.Commands.UpdateFaculty;

using Abstractions.Messaging;
using Core.Models.Faculties.Identifiers;

public sealed record UpdateFacultyCommand(
    FacultyId Id,
    string Name,
    string Colour) 
    : ICommand;