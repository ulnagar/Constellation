namespace Constellation.Application.StaffMembers.GetStaffByEmail;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.StaffMembers.Models;

public sealed record GetStaffByEmailQuery(
    string EmailAddress)
    : IQuery<StaffSelectionListResponse>;