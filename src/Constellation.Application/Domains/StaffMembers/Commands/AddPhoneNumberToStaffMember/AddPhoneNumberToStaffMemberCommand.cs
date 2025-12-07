namespace Constellation.Application.Domains.StaffMember.Commands.AddPhoneNumberToStaffMember

public sealed record AddPhoneNumberToStaffMemberCommand(
    StaffId StaffId,
    PhoneNumber PhoneNumber)
    : ICommand;