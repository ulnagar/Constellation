namespace Constellation.Application.Domains.Tutorials.Queries.GetTutorialRequestsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetTutorialRequestsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<TutorialRequestResponse>>;