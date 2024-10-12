namespace Constellation.Application.SchoolContacts.GetAllSciencePracTeachers;

using Constellation.Application.Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAllSciencePracTeachersQuery()
    : IQuery<List<SchoolContactResponse>>;
