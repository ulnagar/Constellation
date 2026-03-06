namespace Constellation.Application.Domains.Contacts.Queries.GetContactList;

using Abstractions.Messaging;
using Core.Enums;
using Core.Models.Offerings.Identifiers;
using Core.Models.Subjects.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetContactListQuery(
    List<OfferingId> OfferingCodes,
    List<CourseId> CourseIds,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<ContactCategory> ContactCategories,
    List<string> Flags,
    bool IncludeRestrictedRoles)
    : IQuery<List<ContactResponse>>;