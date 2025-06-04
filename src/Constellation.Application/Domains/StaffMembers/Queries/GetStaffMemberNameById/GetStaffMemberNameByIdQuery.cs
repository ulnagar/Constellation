namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffMemberNameById;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;

public sealed record GetStaffMemberNameByIdQuery(
    StaffId StaffId) 
    : IQuery<string>;