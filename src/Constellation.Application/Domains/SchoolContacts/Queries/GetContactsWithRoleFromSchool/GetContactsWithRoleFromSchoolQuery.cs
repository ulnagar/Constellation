namespace Constellation.Application.Domains.SchoolContacts.Queries.GetContactsWithRoleFromSchool;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record GetContactsWithRoleFromSchoolQuery(
    SchoolCode Code,
    bool IncludeRestrictedContacts = false)
    : IQuery<List<ContactResponse>>;