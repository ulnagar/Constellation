namespace Constellation.Application.Domains.AssetManagement.Stocktake.Queries.GetStocktakeSightingsForStaffMember;

using Abstractions.Messaging;
using GetStocktakeSightingsForSchool;
using Models;
using System;
using System.Collections.Generic;

public sealed record GetStocktakeSightingsForStaffMemberQuery(
    string StaffId,
    Guid StocktakeEventId)
    : IQuery<List<StocktakeSightingResponse>>;