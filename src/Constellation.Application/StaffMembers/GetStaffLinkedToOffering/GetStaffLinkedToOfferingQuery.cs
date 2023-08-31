namespace Constellation.Application.StaffMembers.GetStaffLinkedToOffering;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.StaffMembers.Models;
using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetStaffLinkedToOfferingQuery(
    OfferingId OfferingId)
    : IQuery<List<StaffSelectionListResponse>>;