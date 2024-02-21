namespace Constellation.Application.SchoolContacts.RequestContactRemoval;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record RequestContactRemovalCommand(
    SchoolContactId ContactId,
    SchoolContactRoleId RoleId,
    string Comment,
    string CancelledBy,
    string CancelledAt)
    : ICommand;