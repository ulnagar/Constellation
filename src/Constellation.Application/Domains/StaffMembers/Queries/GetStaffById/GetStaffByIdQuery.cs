namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffById;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;

public sealed record GetStaffByIdQuery(
    StaffId StaffId)
    : IQuery<StaffResponse>;
