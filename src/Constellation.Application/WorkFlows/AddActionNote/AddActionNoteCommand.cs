namespace Constellation.Application.WorkFlows.AddActionNote;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;

public sealed record AddActionNoteCommand(
    CaseId CaseId,
    ActionId ActionId,
    string Note)
    : ICommand;