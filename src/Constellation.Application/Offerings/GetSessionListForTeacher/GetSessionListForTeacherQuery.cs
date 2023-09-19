﻿namespace Constellation.Application.Offerings.GetSessionListForTeacher;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetSessionListForTeacherQuery(
    string StaffId)
    : IQuery<List<TeacherSessionResponse>>;
