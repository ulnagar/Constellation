namespace Constellation.Application.SchoolContacts.GetAllSciencePracTeachers;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetAllSciencePracTeachersQuery()
    : IQuery<List<ContactResponse>>;
