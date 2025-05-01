namespace Constellation.Application.Domains.SchoolContacts.Queries.GetAllContacts;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAllContactsQuery(
    GetAllContactsQuery.SchoolContactFilter Filter = GetAllContactsQuery.SchoolContactFilter.WithRole,
    bool IncludeRestrictedRoles = false)
    : IQuery<List<SchoolContactResponse>>
{
    public enum SchoolContactFilter
    {
        All,
        WithRole,
        WithoutRole
    }
}


