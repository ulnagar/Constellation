namespace Constellation.Application.Offerings.GetSessionDetailsForStudent;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;

public sealed record GetSessionDetailsForStudentQuery(
    string StudentId)
    : IQuery<List<StudentSessionDetailsResponse>>;