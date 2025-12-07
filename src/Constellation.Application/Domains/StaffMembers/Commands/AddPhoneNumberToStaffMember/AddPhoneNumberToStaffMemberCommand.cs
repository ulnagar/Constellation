namespace Constellation.Application.Domains.StaffMember.Commands.AddPhoneNumberToStaffMember;

using Constellation.Core.Models.StaffMembers.Identifiers;
using Constellation.Core.ValueObjects;
using Constellation.Application.Abstractions.Messaging;
public sealed record AddPhoneNumberToStaffMemberCommand(
    StaffId StaffId,
    PhoneNumber PhoneNumber)
    : ICommand;
