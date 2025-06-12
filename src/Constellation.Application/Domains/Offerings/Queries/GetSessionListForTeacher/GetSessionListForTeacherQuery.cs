namespace Constellation.Application.Domains.Offerings.Queries.GetSessionListForTeacher;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.StaffMembers.Identifiers;
using System.Collections.Generic;

public sealed record GetSessionListForTeacherQuery(
    StaffId StaffId)
    : IQuery<List<TeacherSessionResponse>>;
