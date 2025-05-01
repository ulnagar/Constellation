namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffById;

using Abstractions.Messaging;

public sealed record GetStaffByIdQuery(
    string StaffId)
    : IQuery<StaffResponse>;
