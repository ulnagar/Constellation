namespace Constellation.Application.SchoolContacts.GetAllContacts;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllContactsQuery(
    GetAllContactsQuery.SchoolContactFilter Filter = GetAllContactsQuery.SchoolContactFilter.WithRole)
    : IQuery<List<SchoolContactResponse>>
{
    public enum SchoolContactFilter
    {
        All,
        WithRole,
        WithoutRole
    }
}


