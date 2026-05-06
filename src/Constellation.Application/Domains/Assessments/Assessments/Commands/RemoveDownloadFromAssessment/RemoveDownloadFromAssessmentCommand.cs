namespace Constellation.Application.Domains.Assessments.Assessments.Commands.RemoveDownloadFromAssessment;

using Abstractions.Messaging;
using Core.Models.Assessments.Identifiers;

public sealed record RemoveDownloadFromAssessmentCommand(
    AssessmentId AssessmentId,
    AssessmentDownloadId DownloadId)
    : ICommand;