namespace Constellation.Application.Domains.SchoolContacts.Commands.RemoveContactRole;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record RemoveContactRoleCommand(
    SchoolContactId ContactId,
    SchoolContactRoleId RoleId)
    : ICommand;