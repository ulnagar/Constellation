namespace Constellation.Application.Users.RepairSchoolContactUser;

using Abstractions.Messaging;
using Models.Identity;

public sealed record RepairSchoolContactUserCommand(
        int SchoolContactId)
    : ICommand<AppUser>;