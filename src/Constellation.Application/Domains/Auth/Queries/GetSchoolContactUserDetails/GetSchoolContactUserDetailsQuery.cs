namespace Constellation.Application.Domains.Auth.Queries.GetSchoolContactUserDetails;

using Abstractions.Messaging;
using GetParentUserDetails;
using System;

public sealed record GetSchoolContactUserDetailsQuery(
    Guid Id)
    : IQuery<ContactUserResponse>;
