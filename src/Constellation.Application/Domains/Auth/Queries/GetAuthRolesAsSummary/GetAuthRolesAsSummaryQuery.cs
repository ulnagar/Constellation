namespace Constellation.Application.Domains.Auth.Queries.GetAuthRolesAsSummary;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAuthRolesAsSummaryQuery()
    : IQuery<List<RoleSummaryResponse>>;