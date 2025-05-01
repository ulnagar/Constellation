namespace Constellation.Application.Domains.Offerings.Queries.GetSessionDetailsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetSessionDetailsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<StudentSessionDetailsResponse>>;