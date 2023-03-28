namespace Constellation.Application.Families.RemoveStudentFromFamily;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record RemoveStudentFromFamilyCommand(
    FamilyId FamilyId,
    string StudentId)
    : ICommand;