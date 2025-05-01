namespace Constellation.Application.Domains.Families.Commands.AddStudentToFamily;

using Abstractions.Messaging;
using Constellation.Core.Models.Identifiers;
using Core.Models.Students.Identifiers;

public sealed record AddStudentToFamilyCommand(
    FamilyId FamilyId,
    StudentId StudentId)
    : ICommand;
