namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsWithAwards;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentStudentsWithAwardsQuery()
    : IQuery<List<CurrentStudentWithAwardsResponse>>;