namespace Constellation.Core.Models.Families.Errors;

using Identifiers;
using Shared;
using System;
using ValueObjects;

public static class FamilyErrors
{
    public static readonly Error EmailAlreadyInUse = new(
        "Families.Family.EmailAlreadyInUse",
        "Email address is already linked to another family");

    public static readonly Func<FamilyId, Error> NotFound = id => new(
        "Families.Family.NotFound",
        $"Could not find a family with Id {id}");

    public static readonly Func<PhoneNumber, Error> NotFoundFromPhoneNumber = number => new(
        "Families.Family.NotFound",
        $"Could not find a family with phone number {number}");

    public static readonly Error InvalidAddress = new(
        "Families.Address.InvalidAddress",
        "The Address supplied is incomplete or invalid");

    public static readonly Error NoneFound = new(
        "Families.Family.NoneFound",
        "Could not find any families");
}
