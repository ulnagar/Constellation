namespace Constellation.Application.Domains.Compliance.Assessments.Commands.SendNotificationsForAssessmentProvisions;

using Constellation.Application.Abstractions.Messaging;

public sealed record SendNotificationsForAssessmentProvisionsCommand()
    : ICommand;