namespace Constellation.Application.Domains.Assessments.Assessments.Commands.SendAssessmentNotificationToSchools;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;

public sealed record SendAssessmentNotificationToSchoolsCommand(
    AssessmentId AssessmentId)
    : ICommand;