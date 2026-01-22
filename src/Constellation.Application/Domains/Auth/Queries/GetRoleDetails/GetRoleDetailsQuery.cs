namespace Constellation.Application.Domains.Auth.Queries.GetRoleDetails;

using Abstractions.Messaging;
using System;

public sealed record GetRoleDetailsQuery(
    Guid RoleId)
    : IQuery<RoleDetailResponse>;