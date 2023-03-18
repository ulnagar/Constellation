namespace Constellation.Application.Families.GetResidentialFamilyEmailAddresses;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.ValueObjects;
using System.Collections.Generic;

public sealed record GetResidentialFamilyEmailAddressesQuery(
    string StudentId)
    : IQuery<List<EmailRecipient>>;
