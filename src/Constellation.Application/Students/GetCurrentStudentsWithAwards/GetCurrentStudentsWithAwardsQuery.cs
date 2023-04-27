namespace Constellation.Application.Students.GetCurrentStudentsWithAwards;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentStudentsWithAwardsQuery()
    : IQuery<List<CurrentStudentWithAwardsResponse>>;