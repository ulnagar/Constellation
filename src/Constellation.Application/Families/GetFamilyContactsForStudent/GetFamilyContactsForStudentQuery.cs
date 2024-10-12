namespace Constellation.Application.Families.GetFamilyContactsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Models;
using System.Collections.Generic;

public sealed record GetFamilyContactsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<FamilyContactResponse>>;