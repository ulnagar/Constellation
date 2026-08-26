namespace Constellation.Application.Domains.Assessments.Assessments.Commands.SendAssessmentNotification;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;

public sealed record SendAssessmentNotificationCommand(
    AssessmentId AssessmentId,
    bool IncludeStudents,
    bool IncludeParents,
    bool IncludeSchoolContacts,
    bool IncludeClassroomTeachers)
    : ICommand;