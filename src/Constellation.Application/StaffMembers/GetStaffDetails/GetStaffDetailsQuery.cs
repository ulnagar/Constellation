namespace Constellation.Application.StaffMembers.GetStaffDetails;

using Abstractions.Messaging;

public sealed record GetStaffDetailsQuery(
    string StaffId)
    : IQuery<StaffDetailsResponse>;