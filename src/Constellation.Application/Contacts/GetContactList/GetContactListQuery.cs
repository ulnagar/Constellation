namespace Constellation.Application.Contacts.GetContactList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Application.Contacts.Models;
using Constellation.Core.Enums;
using Constellation.Core.Models.Offerings.Identifiers;
using System.Collections.Generic;

public sealed record GetContactListQuery(
    List<OfferingId> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<ContactCategory> ContactCategories)
    : IQuery<List<ContactResponse>>;