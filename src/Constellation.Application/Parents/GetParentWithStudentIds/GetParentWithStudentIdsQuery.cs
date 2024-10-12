namespace Constellation.Application.Parents.GetParentWithStudentIds;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetParentWithStudentIdsQuery(
    string ParentEmail)
    : IQuery<List<StudentId>>;