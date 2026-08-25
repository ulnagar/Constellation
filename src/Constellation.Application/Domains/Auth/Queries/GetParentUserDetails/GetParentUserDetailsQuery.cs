namespace Constellation.Application.Domains.Auth.Queries.GetParentUserDetails;

using Abstractions.Messaging;
using System;

public sealed record GetParentUserDetailsQuery(
    Guid Id)
    : IQuery<ParentUserResponse>;
