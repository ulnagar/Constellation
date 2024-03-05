namespace Constellation.Application.ThirdPartyConsent.CreateTransaction;

using Abstractions.Messaging;
using Core.Models.ThirdPartyConsent.Enums;
using System.Collections.Generic;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record CreateTransactionCommand(
    string StudentId,
    string SubmittedBy,
    ConsentMethod SubmissionMethod,
    string Notes,
    Dictionary<ApplicationId, bool> Responses)
    : ICommand;