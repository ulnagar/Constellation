namespace Constellation.Application.Domains.Auth.Commands.RepairSchoolContactUser;

using Abstractions.Messaging;
using Core.Models.Auth;
using Core.Models.SchoolContacts.Identifiers;

public sealed record RepairSchoolContactUserCommand(
    SchoolContactId ContactId)
    : ICommand<AppUser>;