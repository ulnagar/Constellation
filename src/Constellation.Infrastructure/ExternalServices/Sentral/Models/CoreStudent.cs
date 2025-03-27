namespace Constellation.Infrastructure.ExternalServices.Sentral.Models;

using Constellation.Infrastructure.ExternalServices.Sentral.Errors;
using Core.Enums;
using Core.Shared;
using Extensions;
using System;
using System.Text.Json;

public sealed class CoreStudent
{
    public static Result<CoreStudent> ConvertFromJson(JsonElement jsonEntry)
    {
        bool typeExists = jsonEntry.TryGetProperty("type", out JsonElement type);

        if (!typeExists || type.GetString() != "coreStudent")
            return Result.Failure<CoreStudent>(SentralJsonErrors.IncorrectObject("CoreStudent", typeExists ? type.GetString() : string.Empty));

        CoreStudent student = new();

        student.StudentId = jsonEntry.ExtractString("id");

        bool attributesExists = jsonEntry.TryGetProperty("attributes", out JsonElement attributes);
        if (attributesExists)
        {
            student.FirstName = attributes.ExtractString("firstName");
            student.LastName = attributes.ExtractString("lastName");
            student.PreferredName = attributes.ExtractString("preferredName");
            student.Gender = attributes.ExtractString("gender");
            student.StudentReferenceNumber = attributes.ExtractString("externalId");
            student.EmailAddress = attributes.ExtractString("email");
            student.ExternalId = attributes.ExtractGuid("refId");
            student.DateOfBirth = attributes.ExtractDateOnly("dateOfBirth") ?? DateOnly.MinValue;
            student.EnrolDate = attributes.ExtractDateOnly("enrolDate") ?? DateOnly.MinValue;
            student.IsActive = attributes.ExtractBool("isActive") ?? false;
        }

        string grade = attributes.ExtractString("schoolYear");
        if (!string.IsNullOrWhiteSpace(grade))
            student.SchoolYear = Enum.Parse<Grade>(grade);

        bool relationshipsExists = jsonEntry.TryGetProperty("relationships", out JsonElement relationships);
        if (!relationshipsExists)
            return student;
        
        bool familyExists = relationships.TryGetProperty("family", out JsonElement family);
        if (!familyExists)
            return student;

        student.FamilyId = family.GetProperty("data").GetProperty("id").GetString();

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