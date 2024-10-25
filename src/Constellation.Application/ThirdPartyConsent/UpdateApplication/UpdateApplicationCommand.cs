namespace Constellation.Application.ThirdPartyConsent.UpdateApplication;

using Abstractions.Messaging;
using Core.Models.ThirdPartyConsent.Identifiers;

public sealed record UpdateApplicationCommand(
    ApplicationId Id,
    string Name,
    string Purpose,
    string[] InformationCollected,
    string StoredCountry,
    string[] SharedWith,
    string ApplicationLink,
    bool ConsentRequired)
    : ICommand;