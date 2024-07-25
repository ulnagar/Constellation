namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Core.Enums;
using Core.Shared;
using System;

public sealed class CoreStudent
{
    public static Result<CoreStudent> ConvertFromJson(dynamic jsonEntry)
    {
        if (jsonEntry["type"].ToString() != "coreStudent")
            return Result.Failure<CoreStudent>(SentralJsonErrors.IncorrectObject("CoreStudent", jsonEntry["type"].ToString()));

        CoreStudent student = new()
        {
            StudentId = jsonEntry["id"].ToString(),
            FirstName = jsonEntry["attributes"]["firstName"].ToString(),
            LastName = jsonEntry["attributes"]["lastName"].ToString(),
            PreferredName = jsonEntry["attributes"]["preferredName"].ToString(),
            Gender = jsonEntry["attributes"]["gender"].ToString(),
            StudentReferenceNumber = jsonEntry["attributes"]["externalId"].ToString(),
            EmailAddress = jsonEntry["attributes"]["email"].ToString(),
            FamilyId = jsonEntry["relationships"]["family"]["data"]["id"].ToString()
        };

        bool externalIdSuccess = Guid.TryParse(jsonEntry["attributes"]["refId"].ToString(), out Guid externalId);
        if (externalIdSuccess)
            student.ExternalId = externalId;

        bool gradeSuccess = Enum.TryParse<Grade>(jsonEntry["attributes"]["schoolYear"].ToString(), out Grade schoolYear);
        if (gradeSuccess)
            student.SchoolYear = schoolYear;

        bool dobSuccess = DateOnly.TryParse(jsonEntry["attributes"]["dateOfBirth"].ToString(), out DateOnly dateOfBirth);
        if (dobSuccess)
            student.DateOfBirth = dateOfBirth;

        bool enrolDateSuccess = DateOnly.TryParse(jsonEntry["attributes"]["enrolDate"].ToString(), out DateOnly enrolDate);
        if (enrolDateSuccess)
            student.EnrolDate = enrolDate;

        bool activeSuccess = bool.TryParse(jsonEntry["attributes"]["isActive"].ToString(), out bool isActive);
        if (activeSuccess)
            student.IsActive = isActive;

        return student;
    }

    public string StudentId { get; private set; }
    public string FamilyId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string PreferredName { get; private set; }
    public string Gender { get; private set; }
    public Grade SchoolYear { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public string StudentReferenceNumber { get; private set; }
    public string EmailAddress { get; private set; }
    public Guid ExternalId { get; private set; }
    public DateOnly EnrolDate { get; private set; }
    public bool IsActive { get; private set; }
}