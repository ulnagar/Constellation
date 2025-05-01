namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffLinkedToOffering;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetStaffLinkedToOfferingQuery(
    OfferingId OfferingId)
    : IQuery<List<StaffSelectionListResponse>>;