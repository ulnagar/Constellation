namespace Constellation.Application.WorkFlows.UpdateUploadTrainingCertificateAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record UpdateUploadTrainingCertificateActionCommand(
    CaseId CaseId,
    ActionId ActionId)
    : ICommand;