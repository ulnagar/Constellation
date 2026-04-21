namespace Constellation.Application.Domains.Assessments.Assessments.Commands.AddDownloadToAssessment;

using Abstractions.Messaging;
using Constellation.Application.DTOs;
using Core.Models.Assessments.Identifiers;

public sealed record AddDownloadToAssessmentCommand(
    AssessmentId AssessmentId,
    string Name,
    DateOnly AvailableFrom,
    DateOnly AvailableTo,
    bool IsRestricted,
    FileDto File)
    :ICommand;