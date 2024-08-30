namespace Constellation.Application.ThirdPartyConsent.GetConsentStatusByApplication;

using Core.Enums;
using Core.Models.Students.Identifiers;
using Core.ValueObjects;
using System;

public sealed record ConsentStatusResponse(
    StudentId StudentId,
    Name Student,
    Grade Grade,
    string School,
    string ApplicationName,
    ConsentStatusResponse.ConsentStatus Status,
    DateOnly ResponseDate)
{
    public enum ConsentStatus
    {
        Unknown,
        Granted,
        Denied
    }
}