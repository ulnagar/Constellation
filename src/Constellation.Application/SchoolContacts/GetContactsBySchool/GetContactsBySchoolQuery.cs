namespace Constellation.Application.SchoolContacts.GetContactsBySchool;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetContactsBySchoolQuery(
    bool IncludeRestrictedContacts = false)
    : IQuery<List<SchoolWithContactsResponse>>;