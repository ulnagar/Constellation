namespace Constellation.Application.Domains.Students.Queries.GetSchoolEnrolmentHistoryForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetSchoolEnrolmentHistoryForStudentQuery(
    StudentId StudentId)
    : IQuery<List<SchoolEnrolmentResponse>>;