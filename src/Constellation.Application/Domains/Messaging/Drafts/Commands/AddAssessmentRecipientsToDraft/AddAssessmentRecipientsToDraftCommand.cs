namespace Constellation.Application.Domains.Messaging.Drafts.Commands.AddAssessmentRecipientsToDraft;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;
using System;

public sealed record AddAssessmentRecipientsToDraftCommand(
    AssessmentId AssessmentId,
    Guid UserId,
    bool IncludeStudents,
    bool IncludeParents,
    bool IncludeSchoolContacts,
    bool IncludeClassroomTeachers)
    : ICommand;