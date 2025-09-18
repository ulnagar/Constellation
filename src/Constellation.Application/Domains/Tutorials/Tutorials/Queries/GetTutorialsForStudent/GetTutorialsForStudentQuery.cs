namespace Constellation.Application.Domains.Tutorials.Tutorials.Queries.GetTutorialsForStudent;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using System.Collections.Generic;

public sealed record GetTutorialsForStudentQuery(
    StudentId StudentId)
    : IQuery<List<TutorialResponse>>;
