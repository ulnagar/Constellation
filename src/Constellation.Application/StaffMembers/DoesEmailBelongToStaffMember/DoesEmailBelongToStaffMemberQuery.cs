namespace Constellation.Application.StaffMembers.DoesEmailBelongToStaffMember;

using Abstractions.Messaging;

public sealed record DoesEmailBelongToStaffMemberQuery(
    string EmailAddress)
: IQuery<bool>;