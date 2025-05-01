namespace Constellation.Application.Domains.Contacts.Queries.GetContactListForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetContactListForStudentQuery(
    StudentId StudentId)
    : IQuery<List<ContactResponse>>;
