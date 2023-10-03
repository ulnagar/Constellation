namespace Constellation.Application.StaffMembers.GetStaffById;

using Constellation.Application.Abstractions.Messaging;

public sealed record GetStaffByIdQuery(
    string StaffId)
    : IQuery<StaffResponse>;
