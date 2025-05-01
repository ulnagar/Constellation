namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffMemberNameById;

using Abstractions.Messaging;

public sealed record GetStaffMemberNameByIdQuery(
    string StaffId) 
    : IQuery<string>;