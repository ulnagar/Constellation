namespace Constellation.Application.Contacts.GetContactListForStudent;

using Abstractions.Messaging;
using Constellation.Application.Contacts.Models;
using System.Collections.Generic;

public sealed record GetContactListForStudentQuery(
    string StudentId)
    : IQuery<List<ContactResponse>>;
