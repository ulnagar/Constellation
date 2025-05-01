namespace Constellation.Application.Domains.StaffMembers.Queries.DoesEmailBelongToStaffMember;

using Abstractions.Messaging;

public sealed record DoesEmailBelongToStaffMemberQuery(
    string EmailAddress)
: IQuery<bool>;