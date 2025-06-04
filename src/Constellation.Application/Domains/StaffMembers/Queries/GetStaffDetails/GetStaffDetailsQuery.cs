namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffDetails;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;

public sealed record GetStaffDetailsQuery(
    StaffId StaffId)
    : IQuery<StaffDetailsResponse>;