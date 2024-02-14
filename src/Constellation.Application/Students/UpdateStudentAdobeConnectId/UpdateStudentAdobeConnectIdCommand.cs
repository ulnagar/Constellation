namespace Constellation.Application.Students.UpdateStudentAdobeConnectId;

using Abstractions.Messaging;

public sealed record UpdateStudentAdobeConnectIdCommand(
    string StudentId)
: ICommand<string>;