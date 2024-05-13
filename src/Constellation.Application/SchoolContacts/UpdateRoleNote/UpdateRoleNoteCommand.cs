namespace Constellation.Application.SchoolContacts.UpdateRoleNote;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record UpdateRoleNoteCommand(
    SchoolContactId ContactId,
    SchoolContactRoleId RoleId,
    string Note)
    : ICommand;