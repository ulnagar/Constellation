namespace Constellation.Application.Domains.StaffMembers.Models;

using Core.Models.StaffMembers.Identifiers;
using Core.ValueObjects;

public sealed record StaffSelectionListResponse(
    StaffId StaffId,
    Name Name);
