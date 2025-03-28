namespace Constellation.Application.OfferingEnrolments.GetStudentEnrolmentsWithDetails;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetStudentEnrolmentsWithDetailsQuery(
    StudentId StudentId)
    : IQuery<List<StudentEnrolmentResponse>>;