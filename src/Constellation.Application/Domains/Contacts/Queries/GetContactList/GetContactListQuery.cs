namespace Constellation.Application.Domains.Contacts.Queries.GetContactList;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetContactListQuery(
    ContactFilter Filter,
    bool IncludeRestrictedRoles)
    : IQuery<List<ContactResponse>>;