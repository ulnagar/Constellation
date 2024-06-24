namespace Constellation.Application.Training.GetListOfStaffMembersWithoutModule;

using Core.ValueObjects;

public sealed record StaffResponse(
    string StaffId,
    Name StaffName);