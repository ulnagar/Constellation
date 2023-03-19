namespace Constellation.Application.Students.WithdrawStudent;

using Constellation.Application.Abstractions.Messaging;

public sealed record WithdrawStudentCommand(
    string StudentId)
    : ICommand;
