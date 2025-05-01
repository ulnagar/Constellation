namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffDetails;

using Abstractions.Messaging;

public sealed record GetStaffDetailsQuery(
    string StaffId)
    : IQuery<StaffDetailsResponse>;