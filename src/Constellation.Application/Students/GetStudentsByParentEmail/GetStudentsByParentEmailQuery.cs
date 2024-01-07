namespace Constellation.Application.Students.GetStudentsByParentEmail;

using Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStudentsByParentEmailQuery(
        string ParentEmail)
    : IQuery<List<StudentResponse>>;