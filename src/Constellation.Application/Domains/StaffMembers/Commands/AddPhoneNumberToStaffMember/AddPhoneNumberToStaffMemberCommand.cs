namespace Constellation.Application.Domains.StaffMembers.Commands.AddPhoneNumberToStaffMember;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.ValueObjects;

public sealed record AddPhoneNumberToStaffMemberCommand(
    StaffId StaffId,
    PhoneNumber PhoneNumber)
    : ICommand;
