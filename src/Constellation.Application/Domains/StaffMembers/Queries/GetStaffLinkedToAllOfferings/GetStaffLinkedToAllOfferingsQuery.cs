namespace Constellation.Application.Domains.StaffMembers.Queries.GetStaffLinkedToAllOfferings;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStaffLinkedToAllOfferingsQuery()
    : IQuery<List<OfferingStaffResponse>>;