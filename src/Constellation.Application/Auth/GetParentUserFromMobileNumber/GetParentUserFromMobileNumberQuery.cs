namespace Constellation.Application.Auth.GetParentUserFromMobileNumber;

using Abstractions.Messaging;
using Core.ValueObjects;
using Models.Identity;

public sealed record GetParentUserFromMobileNumberQuery(
    PhoneNumber PhoneNumber)
    : IQuery<AppUser>;