namespace Constellation.Application.WorkFlows.UpdateSendEmailAction;

using Abstractions.Messaging;
using Core.Models.WorkFlow.Identifiers;
using Core.ValueObjects;
using DTOs;
using System.Collections.Generic;

public sealed record UpdateSendEmailActionCommand(
    CaseId CaseId,
    ActionId ActionId,
    List<EmailRecipient> Recipients,
    EmailRecipient Sender,
    string Subject,
    string Body,
    List<FileDto> Attachments)
    : ICommand;