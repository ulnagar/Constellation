namespace Constellation.Application.Domains.Auth.Queries.GetParentUserFromMobileNumber;

using Abstractions.Messaging;
using Core.ValueObjects;
using Models.Identity;

public sealed record GetParentUserFromMobileNumberQuery(
    PhoneNumber PhoneNumber)
    : IQuery<AppUser>;