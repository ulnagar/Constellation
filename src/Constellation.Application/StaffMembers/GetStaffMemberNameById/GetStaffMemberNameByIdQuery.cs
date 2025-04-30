namespace Constellation.Application.StaffMembers.GetStaffMemberNameById;

using Abstractions.Messaging;

public sealed record GetStaffMemberNameByIdQuery(
    string StaffId) 
    : IQuery<string>;