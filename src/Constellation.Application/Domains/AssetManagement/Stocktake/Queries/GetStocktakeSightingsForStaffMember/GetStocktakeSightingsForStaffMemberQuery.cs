namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingsForStaffMember;

using Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using Core.Models.Stocktake.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetStocktakeSightingsForStaffMemberQuery(
    StaffId StaffId,
    StocktakeEventId StocktakeEventId)
    : IQuery<List<StocktakeSightingResponse>>;