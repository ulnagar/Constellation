namespace Constellation.Application.SchoolContacts.RemoveContactRole;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record RemoveContactRoleCommand(
    SchoolContactId ContactId,
    SchoolContactRoleId RoleId)
    : ICommand;