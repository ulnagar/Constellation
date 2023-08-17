namespace Constellation.Application.Contacts.GetContactListForParentPortal;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetContactListForParentPortalQuery(
    string StudentId)
    : IQuery<List<StudentSupportContactResponse>>;
