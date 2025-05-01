namespace Constellation.Application.Domains.Families.Commands.RemoveStudentFromFamily;

using Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record RemoveStudentFromFamilyCommand(
    FamilyId FamilyId,
    StudentId StudentId)
    : ICommand;