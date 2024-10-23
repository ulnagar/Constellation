namespace Constellation.Core.Models.ThirdPartyConsent.Errors;

using Constellation.Core.Models.Students.Identifiers;
using Identifiers;
using Shared;
using System;

public static class ConsentErrors
{
    public static readonly Func<ConsentId, Error> NotFound = id => new(
        "Consent.Consent.NotFound",
        $"Could not find a Consent Response with Id {id.Value}");

    public static readonly Func<string, StudentId, Error> NoneFoundForStudentAndApplication =
        (application, studentId) => new(
            "Consent.Consent.NoneFoundForStudentAndApplication",
            $"Could not find any Consent Response for Application \"{application}\" and student with Id {studentId}");
}