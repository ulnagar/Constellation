namespace Constellation.Application.Domains.Families.Queries.GetFamilyContactsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetFamilyContactsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<FamilyContactResponse>>;