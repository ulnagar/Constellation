namespace Constellation.Application.Domains.Families.Queries.GetFamilyContacts;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Domains.Families.Models;
using System.Collections.Generic;

public sealed record GetFamilyContactsQuery()
    : IQuery<List<FamilyContactResponse>>;