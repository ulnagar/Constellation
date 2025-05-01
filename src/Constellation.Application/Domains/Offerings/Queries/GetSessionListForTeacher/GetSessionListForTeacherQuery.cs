namespace Constellation.Application.Domains.Offerings.Queries.GetSessionListForTeacher;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetSessionListForTeacherQuery(
    string StaffId)
    : IQuery<List<TeacherSessionResponse>>;
