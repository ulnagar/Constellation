namespace Constellation.Application.SchoolContacts.GetAllContacts;

using Abstractions.Messaging;
using Constellation.Application.SchoolContacts.Models;
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


