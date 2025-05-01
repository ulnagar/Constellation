namespace Constellation.Application.Domains.SchoolContacts.Queries.GetAllSciencePracTeachers;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetAllSciencePracTeachersQuery()
    : IQuery<List<SchoolContactResponse>>;
