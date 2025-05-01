namespace Constellation.Application.Domains.Training.Queries.GetListOfStaffMembersWithoutModule;

using Core.ValueObjects;

public sealed record StaffResponse(
    string StaffId,
    Name StaffName);