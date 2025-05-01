namespace Constellation.Application.Domains.WorkFlows.Commands.UpdateUploadTrainingCertificateAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record UpdateUploadTrainingCertificateActionCommand(
    CaseId CaseId,
    ActionId ActionId)
    : ICommand;