namespace Constellation.Application.SchoolContacts.GetContactsWithRoleFromSchool;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetContactsWithRoleFromSchoolQuery(
    string Code,
    bool IncludeRestrictedContacts = false)
    : IQuery<List<ContactResponse>>;