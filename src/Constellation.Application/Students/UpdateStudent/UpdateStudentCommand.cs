namespace Constellation.Application.Students.UpdateStudent;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Students.Enums;
using Core.Models.Students.Identifiers;
using Core.Models.Students.ValueObjects;
using Core.ValueObjects;

public sealed record UpdateStudentCommand(
    StudentId StudentId,
    StudentReferenceNumber SRN,
    Name Name,
    Grade CurrentGrade,
    EmailAddress EmailAddress,
    Gender Gender)
    : ICommand;