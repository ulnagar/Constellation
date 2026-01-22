namespace Constellation.Application.Domains.Auth.Queries.GetUserDetails;

using Abstractions.Messaging;
using System;

public sealed record GetUserDetailsQuery(
    Guid Id)
    : IQuery<UserResponse>;
