namespace Constellation.Application.Contacts.GetContactList;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Enums;
using System.Collections.Generic;

public sealed record GetContactListQuery(
    List<int> OfferingCodes,
    List<Grade> Grades,
    List<string> SchoolCodes,
    List<ContactCategory> ContactCateogries)
    : IQuery<List<ContactResponse>>;