namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffByEmail;

using Abstractions.Messaging;
using Models;

public sealed record GetStaffByEmailQuery(
    string EmailAddress)
    : IQuery<StaffSelectionListResponse>;