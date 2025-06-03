namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingsForStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Models;
using System;
using System.Collections.Generic;

public sealed record GetStocktakeSightingsForStaffMemberQuery(
    StaffId StaffId,
    Guid StocktakeEventId)
    : IQuery<List<StocktakeSightingResponse>>;