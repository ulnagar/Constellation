namespace Constellation.Application.Users.RepairSchoolContactUser;

using Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;
using Models.Identity;

public sealed record RepairSchoolContactUserCommand(
    SchoolContactId ContactId)
    : ICommand<AppUser>;