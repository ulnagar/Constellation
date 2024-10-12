namespace Constellation.Application.ThirdPartyConsent.CreateTransaction;

using Abstractions.Messaging;
using Core.Models.Students.Identifiers;
using Core.Models.ThirdPartyConsent.Enums;
using System.Collections.Generic;
using ApplicationId = Core.Models.ThirdPartyConsent.Identifiers.ApplicationId;

public sealed record CreateTransactionCommand(
    StudentId StudentId,
    string SubmittedBy,
    ConsentMethod SubmissionMethod,
    string Notes,
    Dictionary<ApplicationId, bool> Responses)
    : ICommand;