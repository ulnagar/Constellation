namespace Constellation.Application.Families.GetResidentialFamilyMobileNumbers;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;

public sealed record GetResidentialFamilyMobileNumbersQuery(
    string StudentId)
    : IQuery<List<PhoneNumber>>;