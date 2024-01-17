namespace Constellation.Core.Models.ThirdPartyConsent;

using Primitives;
using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationId = Identifiers.ApplicationId;

public sealed class Application : IAuditableEntity
{
    private readonly List<Consent> _consents = new();

    private Application() { } // Required by EF Core

    private Application(
        string name,
        string purpose,
        string[] informationCollected,
        string storedCountry,
        string[] sharedWith,
        bool consentRequired)
    {
        Id = new();
        Name = name;
        Purpose = purpose;
        InformationCollected = informationCollected;
        StoredCountry = storedCountry;
        SharedWith = sharedWith;
        ConsentRequired = consentRequired;
    }

    public ApplicationId Id { get; private set; }
    public string Name { get; private set; }
    public string Purpose { get; private set; }
    public string[] InformationCollected { get; private set; }
    public string StoredCountry { get; private set; }
    public string[] SharedWith { get; private set; }
    public bool ConsentRequired { get; private set; }
    public IReadOnlyList<Consent> Consents => _consents.ToList();

    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsDeleted { get; private set; }
    public string DeletedBy { get; set; }
    public DateTime DeletedAt { get; set; }

    public static Application Create(
        string name,
        string purpose,
        string[] informationCollected,
        string storedCountry,
        string[] sharedWith,
        bool consentRequired)
    {
        Application application = new(
            name,
            purpose,
            informationCollected,
            storedCountry,
            sharedWith,
            consentRequired);

        return application;
    }

    public Result Update(
        string name,
        string purpose,
        string[] informationCollected,
        string storedCountry,
        string[] sharedWith,
        bool consentRequired)
    {
        Name = name;
        Purpose = purpose;
        InformationCollected = informationCollected;
        StoredCountry = storedCountry;
        SharedWith = sharedWith;
        ConsentRequired = consentRequired;

        return Result.Success();
    }
    
    public List<Consent> GetActiveConsents()
    {
        List<string> studentIds = _consents
            .Select(consent => consent.StudentId)
            .Distinct()
            .ToList();

        return studentIds
            .Select(studentId => 
                _consents
                    .Where(consent => consent.StudentId == studentId)
                    .MaxBy(consent => consent.ProvidedAt))
            .ToList();
    }

    public void Delete() => IsDeleted = true;
}