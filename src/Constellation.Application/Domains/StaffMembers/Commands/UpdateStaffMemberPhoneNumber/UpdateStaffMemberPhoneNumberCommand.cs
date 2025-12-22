namespace Constellation.Application.Domains.StaffMembers.Commands.UpdateStaffMemberPhoneNumber;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;

public sealed record UpdateStaffMemberPhoneNumberCommand(
    StaffId StaffId,
    PhoneNumber PhoneNumber)
    : ICommand;
