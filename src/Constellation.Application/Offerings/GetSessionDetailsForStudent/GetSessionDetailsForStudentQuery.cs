namespace Constellation.Application.Offerings.GetSessionDetailsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetSessionDetailsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<StudentSessionDetailsResponse>>;