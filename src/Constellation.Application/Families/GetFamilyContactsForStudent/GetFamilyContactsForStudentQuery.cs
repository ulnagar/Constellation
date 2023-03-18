namespace Constellation.Application.Families.GetFamilyContactsForStudent;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetFamilyContactsForStudentQuery(
    string StudentId)
    : IQuery<List<FamilyContactResponse>>;