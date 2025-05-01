namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsWithSentralId;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetCurrentStudentsWithSentralIdQuery()
    : IQuery<List<StudentWithSentralIdResponse>>;