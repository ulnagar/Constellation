namespace Constellation.Application.Enrolments.GetStudentEnrolmentsWithDetails;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetStudentEnrolmentsWithDetailsQuery(
    string StudentId)
    : IQuery<List<StudentEnrolmentResponse>>;