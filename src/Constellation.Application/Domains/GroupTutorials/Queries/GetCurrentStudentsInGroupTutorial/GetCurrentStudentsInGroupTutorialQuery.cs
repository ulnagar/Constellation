namespace Constellation.Application.Domains.GroupTutorials.Queries.GetCurrentStudentsInGroupTutorial;

using Abstractions.Messaging;
using Constellation.Application.Domains.Students.Models;
using Core.Models.Identifiers;
using System.Collections.Generic;

public sealed record GetCurrentStudentsInGroupTutorialQuery(
    GroupTutorialId TutorialId)
    : IQuery<List<StudentResponse>>;
