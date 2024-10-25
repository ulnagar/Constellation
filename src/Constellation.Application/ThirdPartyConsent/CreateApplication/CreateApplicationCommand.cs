namespace Constellation.Application.ThirdPartyConsent.CreateApplication;

using Constellation.Application.Abstractions.Messaging;
using Core.Models.ThirdPartyConsent.Identifiers;

public sealed record CreateApplicationCommand(
    string Name,
    string Purpose,
    string[] InformationCollected,
    string StoredCountry,
    string[] SharedWith,
    string ApplicationLink,
    bool ConsentRequired)
    : ICommand<ApplicationId>;
