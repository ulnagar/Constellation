namespace Constellation.Infrastructure.Services;

using Application.Common.Errors;
using Application.Domains.Import.Helpers;
using Application.Domains.Import.Interfaces;
using Application.Interfaces.Repositories;
using Application.Models.ImportCache;
using Core.Enums;
using Core.Models;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.Identifiers;
using Core.Models.Students.Enums;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using System.Collections.Generic;

internal sealed class ApplicationImportRowMapper : IImportRowMapper<Application, EnrolmentPeriod>
{
    private readonly ISchoolRepository _schoolRepository;

    public ApplicationImportRowMapper(
        ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<Application>> Map(
        StagedImportRow row, 
        IReadOnlyDictionary<string, string?> columnMapping,
        EnrolmentPeriod period,
        CancellationToken cancellationToken = default)
    {
        string? Get(string fieldKey) => ImportRowValueAccessor.Get(row, columnMapping, fieldKey);

        string? studentReferenceNumber = Get("StudentReferenceNumber");
        string? studentNameFirst = Get("StudentName.First");
        string? studentNamePreferred = Get("StudentName.Preferred");
        string? studentNameLast = Get("StudentName.Last");
        string? dateOfBirth = Get("DateOfBirth");
        string? studentEmailAddress = Get("StudentEmailAddress");
        string? parentNameFirst = Get("ParentName.First");
        string? parentNameLast = Get("ParentName.Last");
        string? parentEmailAddress = Get("ParentEmailAddress");
        string? parentPhoneNumber = Get("ParentPhoneNumber");
        string? mailingAddressStreet = Get("MailingAddress.Street");
        string? mailingAddressTown = Get("MailingAddress.Town");
        string? mailingAddressState = Get("MailingAddress.State");
        string? mailingAddressPostcode = Get("MailingAddress.Postcode");
        string? applicationReference = Get("ApplicationReference");
        string? currentSchoolName = Get("CurrentSchoolName");
        string? destinationSchoolName = Get("DestinationSchoolName");
        string? grade = Get("Grade");

        if (string.IsNullOrWhiteSpace(studentNameFirst))
            return Result.Failure<Application>(ImportErrors.RequiredFieldMissing("Student First Name", row.RowNumber));

        if (string.IsNullOrWhiteSpace(studentNameLast))
            return Result.Failure<Application>(ImportErrors.RequiredFieldMissing("Student Last Name", row.RowNumber));

        if (string.IsNullOrWhiteSpace(grade))
            return Result.Failure<Application>(ImportErrors.RequiredFieldMissing("Grade", row.RowNumber));

        Result<Name> studentNameResult = Name.Create(
            studentNameFirst,
            studentNamePreferred ?? string.Empty,
            studentNameLast);

        if (studentNameResult.IsFailure)
            return Result.Failure<Application>(studentNameResult.Error);

        bool gradeResult = Enum.TryParse(grade, out Grade gradeValue);

        if (!gradeResult)
            return Result.Failure<Application>(ImportErrors.ValueParseError(typeof(Grade), "Grade"));

        StudentReferenceNumber? srn = null;
        if (!string.IsNullOrWhiteSpace(studentReferenceNumber))
        {
            Result<StudentReferenceNumber> srnResult = StudentReferenceNumber.Create(studentReferenceNumber);

            if (srnResult.IsFailure)
                return Result.Failure<Application>(srnResult.Error);

            srn = srnResult.Value;
        }

        Name? parentName = null;
        if (!string.IsNullOrWhiteSpace(parentNameFirst) && !string.IsNullOrWhiteSpace(parentNameLast))
        {
            Result<Name> parentNameResult = Name.Create(
                parentNameFirst,
                string.Empty,
                parentNameLast);

            if (parentNameResult.IsFailure)
                return Result.Failure<Application>(parentNameResult.Error);

            parentName = parentNameResult.Value;
        }
        
        DateOnly? birthDate = null;
        if (!string.IsNullOrWhiteSpace(dateOfBirth))
        {
            bool dateOfBirthResult = DateOnly.TryParse(dateOfBirth, out DateOnly birthDateTemp);

            if (!dateOfBirthResult)
                return Result.Failure<Application>(ImportErrors.ValueParseError(typeof(DateOnly), "Date Of Birth"));

            birthDate = birthDateTemp;
        }

        EmailAddress? studentEmail = null;
        if (!string.IsNullOrWhiteSpace(studentEmailAddress))
        {
            Result<EmailAddress> studentEmailAddressResult = EmailAddress.Create(studentEmailAddress);

            if (studentEmailAddressResult.IsFailure)
                return Result.Failure<Application>(studentEmailAddressResult.Error);

            studentEmail = studentEmailAddressResult.Value;
        }

        EmailAddress? parentEmail = null;
        if (!string.IsNullOrWhiteSpace(parentEmailAddress))
        {
            Result<EmailAddress> parentEmailAddressResult = EmailAddress.Create(parentEmailAddress);

            if (parentEmailAddressResult.IsFailure)
                return Result.Failure<Application>(parentEmailAddressResult.Error);

            parentEmail = parentEmailAddressResult.Value;
        }

        PhoneNumber? parentPhone = null;
        if (!string.IsNullOrWhiteSpace(parentPhoneNumber))
        {
            Result<PhoneNumber> parentPhoneNumberResult = PhoneNumber.Create(parentPhoneNumber);

            if (parentPhoneNumberResult.IsFailure)
                return Result.Failure<Application>(parentPhoneNumberResult.Error);

            parentPhone = parentPhoneNumberResult.Value;
        }

        MailingAddress? address = null;
        if (!string.IsNullOrWhiteSpace(mailingAddressStreet)
            && !string.IsNullOrWhiteSpace(mailingAddressTown)
            && !string.IsNullOrWhiteSpace(mailingAddressState)
            && !string.IsNullOrWhiteSpace(mailingAddressPostcode))
        {
            Result<MailingAddress> mailingAddressResult = MailingAddress.Create(
                mailingAddressStreet,
                mailingAddressTown,
                mailingAddressState,
                mailingAddressPostcode);

            if (mailingAddressResult.IsFailure)
                return Result.Failure<Application>(mailingAddressResult.Error);

            address = mailingAddressResult.Value;
        }

        SchoolCode? currentSchoolCode = null;
        string? currentSchool = null;
        if (!string.IsNullOrWhiteSpace(currentSchoolName))
        {
            School? foundSchool = await _schoolRepository.GetByName(currentSchoolName, cancellationToken);

            if (foundSchool is not null)
                currentSchoolCode = foundSchool.Code;

            currentSchool = currentSchoolName;
        }

        SchoolCode? destinationSchoolCode = null;
        string? destinationSchool = null;
        if (!string.IsNullOrWhiteSpace(destinationSchoolName))
        {
            School? foundSchool = await _schoolRepository.GetByName(destinationSchoolName, cancellationToken);

            if (foundSchool is null)
                return Result.Failure<Application>(ImportErrors.ValueParseError(typeof(School), "Destination School"));

            destinationSchoolCode = foundSchool.Code;
            destinationSchool = foundSchool.Name;
        }

        return Application.Create(
            period.Id,
            srn,
            studentNameResult.Value,
            Gender.Empty,
            birthDate,
            studentEmail,
            parentName,
            parentEmail,
            parentPhone,
            address,
            applicationReference,
            currentSchoolCode,
            currentSchool,
            destinationSchoolCode,
            destinationSchool,
            period.Program,
            gradeValue);
    }
}
