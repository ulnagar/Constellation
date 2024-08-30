﻿namespace Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetStudentEnrolmentsWithDetailsQuery(
    StudentId StudentId)
    : IQuery<List<StudentEnrolmentResponse>>;