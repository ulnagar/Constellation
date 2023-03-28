namespace Constellation.Application.Families.AddStudentToFamily;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;

public sealed record AddStudentToFamilyCommand(
    FamilyId FamilyId,
    string StudentId)
    : ICommand;
