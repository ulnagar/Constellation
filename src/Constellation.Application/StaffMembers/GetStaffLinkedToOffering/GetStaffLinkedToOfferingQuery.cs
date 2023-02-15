namespace Constellation.Application.StaffMembers.GetStaffLinkedToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.StaffMembers.Models;
using System.Collections.Generic;

public sealed record GetStaffLinkedToOfferingQuery(
    int OfferingId)
    : IQuery<List<StaffSelectionListResponse>>;