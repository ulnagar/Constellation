namespace Constellation.Application.Contacts.GetContactListForParentPortal;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetContactListForParentPortalQuery(
    StudentId StudentId)
    : IQuery<List<StudentSupportContactResponse>>;
