namespace Constellation.Application.Domains.Students.Commands.UpdateStudentSentralId;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record UpdateStudentSentralIdCommand(
    StudentId StudentId)
    : ICommand;