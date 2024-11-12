namespace Constellation.Application.GroupTutorials.GetCurrentStudentsInGroupTutorial;

using Abstractions.Messaging;
using Core.Models.Identifiers;
using Students.Models;
using System.Collections.Generic;

public sealed record GetCurrentStudentsInGroupTutorialQuery(
    GroupTutorialId TutorialId)
    : IQuery<List<StudentResponse>>;
