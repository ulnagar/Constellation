namespace Constellation.Application.Parents.GetParentWithStudentIds;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetParentWithStudentIdsQuery(
    string ParentEmail)
    : IQuery<List<string>>;