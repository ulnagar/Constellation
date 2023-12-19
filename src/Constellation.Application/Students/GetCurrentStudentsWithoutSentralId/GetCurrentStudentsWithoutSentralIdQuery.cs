namespace Constellation.Application.Students.GetCurrentStudentsWithoutSentralId;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetCurrentStudentsWithoutSentralIdQuery()
    : IQuery<List<StudentResponse>>;
