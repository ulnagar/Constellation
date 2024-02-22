namespace Constellation.Application.AdminDashboards.RepairSchoolContactUser;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.SchoolContacts.Identifiers;

public sealed record RepairSchoolContactUserCommand(
    SchoolContactId ContactId)
    : ICommand;
