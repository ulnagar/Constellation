namespace Constellation.Application.SchoolContacts.RemoveContactRole;

using Constellation.Application.Abstractions.Messaging;

public sealed record RemoveContactRoleCommand(
    int RoleId)
    : ICommand;