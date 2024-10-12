namespace Constellation.Application.Students.WithdrawStudent;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.Students.Identifiers;

public sealed record WithdrawStudentCommand(
    StudentId StudentId)
    : ICommand;
