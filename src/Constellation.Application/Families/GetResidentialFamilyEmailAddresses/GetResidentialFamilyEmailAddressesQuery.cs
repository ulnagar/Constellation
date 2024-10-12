namespace Constellation.Application.Families.GetResidentialFamilyEmailAddresses;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System.Collections.Generic;

public sealed record GetResidentialFamilyEmailAddressesQuery(
    StudentId StudentId)
    : IQuery<List<EmailRecipient>>;
