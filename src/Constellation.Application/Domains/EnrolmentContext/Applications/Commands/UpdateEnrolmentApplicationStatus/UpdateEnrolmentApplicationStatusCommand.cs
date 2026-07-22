namespace Constellation.Application.Domains.EnrolmentContext.Applications.Commands.UpdateEnrolmentApplicationStatus;

using Abstractions.Messaging;
using Core.Models.EnrolmentContext.Application.Enums;
using ApplicationId = Core.Models.EnrolmentContext.Application.Identifiers.ApplicationId;

public sealed record UpdateEnrolmentApplicationStatusCommand(
    ApplicationId ApplicationId,
    ApplicationStatus Status)
    : ICommand;