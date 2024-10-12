namespace Constellation.Application.Families.GetResidentialFamilyMobileNumbers;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record GetResidentialFamilyMobileNumbersQuery(
    StudentId StudentId)
    : IQuery<List<PhoneNumber>>;