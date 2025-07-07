namespace Constellation.Application.Domains.Training.Queries.GetListOfStaffMembersWithoutModule;

using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;

public sealed record StaffResponse(
    StaffId StaffId,
    Name StaffName);