namespace Constellation.Core.Models.Families.Errors;

using Identifiers;
using Shared;
using System;
using ValueObjects;

public static class ParentErrors
{
    public static readonly Func<ParentId, FamilyId, Error> NotFoundInFamily = (parentId, familyId) => new(
        "Families.Parent.NotFoundInFamily",
        $"Could not find a parent with Id {parentId} in the family with Id {familyId}");

    public static readonly Error AlreadyExists = new(
        "Families.Parent.AlreadyExists",
        "Cannot create a new parent as another parent already exists with these details");

    public static readonly Func<PhoneNumber, Error> TooManyWithSameNumber = number => new(
        "Families.Parent.TooManyWithSameNumber",
        $"Found too many parents with the same mobile number {number}");

    public static readonly Func<PhoneNumber, Error> NotFoundWithNumber = number => new(
        "Families.Parent.NotFoundWithNumber",
        $"Could not find a parent with mobile number {number}");

}
