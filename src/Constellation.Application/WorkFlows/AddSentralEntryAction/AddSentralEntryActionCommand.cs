namespace Constellation.Application.WorkFlows.AddSentralEntryAction;

using Abstractions.Messaging;
using Core.Models.Offerings.Identifiers;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddSentralEntryActionCommand(
    CaseId CaseId,
    OfferingId OfferingId,
    string StaffId)
    : ICommand;
