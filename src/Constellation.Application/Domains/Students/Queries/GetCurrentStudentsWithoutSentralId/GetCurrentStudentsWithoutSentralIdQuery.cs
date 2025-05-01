namespace Constellation.Application.Domains.Students.Queries.GetCurrentStudentsWithoutSentralId;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentStudentsWithoutSentralIdQuery()
    : IQuery<List<StudentResponse>>;
