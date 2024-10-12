namespace Constellation.Application.Users.AuditAllUsers;

using Abstractions.Messaging;

public sealed record AuditAllUsersCommand()
    : ICommand;