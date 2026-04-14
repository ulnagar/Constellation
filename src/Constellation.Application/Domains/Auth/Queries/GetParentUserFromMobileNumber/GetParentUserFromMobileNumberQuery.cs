namespace Constellation.Application.Domains.Auth.Queries.GetParentUserFromMobileNumber;

using Abstractions.Messaging;
using Core.Models.Auth;
using Core.ValueObjects;

public sealed record GetParentUserFromMobileNumberQuery(
    PhoneNumber PhoneNumber)
    : IQuery<AppUser>;