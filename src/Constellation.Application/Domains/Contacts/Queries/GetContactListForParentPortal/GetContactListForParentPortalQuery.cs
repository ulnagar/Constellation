namespace Constellation.Application.Domains.Contacts.Queries.GetContactListForParentPortal;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetContactListForParentPortalQuery(
    StudentId StudentId)
    : IQuery<List<StudentSupportContactResponse>>;
