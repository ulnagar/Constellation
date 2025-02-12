namespace Constellation.Application.Students.SetStudentPhoto;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record SetStudentPhotoCommand(
    StudentId StudentId,
    byte[] Photo)
    : ICommand;