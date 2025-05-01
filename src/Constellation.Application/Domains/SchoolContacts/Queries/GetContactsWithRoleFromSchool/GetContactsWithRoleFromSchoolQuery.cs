namespace Constellation.Application.Domains.SchoolContacts.Queries.GetContactsWithRoleFromSchool;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetContactsWithRoleFromSchoolQuery(
    string Code,
    bool IncludeRestrictedContacts = false)
    : IQuery<List<ContactResponse>>;