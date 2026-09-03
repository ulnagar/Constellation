namespace Constellation.Infrastructure.Services;

using Application.Common.Errors;
using Application.Domains.Import.Helpers;
using Application.Domains.Import.Interfaces;
using Application.Domains.Import.Models;
using Application.Interfaces.Repositories;
using Application.Models.ImportCache;
using Core.Enums;
using Core.Models;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.EnrolmentPeriod;
using Core.Models.Identifiers;
using Core.Models.Students.Enums;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Globalization;

internal sealed class ApplicationImportRowMapper : IImportRowMapper<Application, EnrolmentPeriod>
{
    private readonly ISchoolRepository _schoolRepository;

    public ApplicationImportRowMapper(
        ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<Application>> MapNew(
        StagedImportRow row,
        IReadOnlyDictionary<string, string?> columnMapping,
        EnrolmentPeriod period,
        CancellationToken cancellationToken = default)
    {
        string? Get(string fieldKey) => ImportRowValueAccessor.Get(row, columnMapping, fieldKey);

        string? studentReferenceNumber = Get(EnrolmentApplicationImportFields.StudentReferenceNumber);
        string? studentNameFirst = Get(EnrolmentApplicationImportFields.StudentNameFirst);
        string? studentNamePreferred = Get(EnrolmentApplicationImportFields.StudentNamePreferred);
        string? studentNameLast = Get(EnrolmentApplicationImportFields.StudentNameLast);
        string? dateOfBirth = Get(EnrolmentApplicationImportFields.DateOfBirth);
        string? genderString = Get(EnrolmentApplicationImportFields.Gender);
        string? studentEmailAddress = Get(EnrolmentApplicationImportFields.StudentEmailAddress);
        string? parentNameFirst = Get(EnrolmentApplicationImportFields.ParentNameFirst);
        string? parentNameLast = Get(EnrolmentApplicationImportFields.ParentNameLast);
        string? parentEmailAddress = Get(EnrolmentApplicationImportFields.ParentEmailAddress);
        string? parentPhoneNumber = Get(EnrolmentApplicationImportFields.ParentPhoneNumber);
        string? mailingAddressStreet = Get(EnrolmentApplicationImportFields.MailingAddressStreet);
        string? mailingAddressTown = Get(EnrolmentApplicationImportFields.MailingAddressTown);
        string? mailingAddressState = Get(EnrolmentApplicationImportFields.MailingAddressState);
        string? mailingAddressPostcode = Get(EnrolmentApplicationImportFields.MailingAddressPostcode);
        string? applicationReference = Get(EnrolmentApplicationImportFields.ApplicationReference);
        string? currentSchoolName = Get(EnrolmentApplicationImportFields.CurrentSchoolName);
        string? destinationSchoolName = Get(EnrolmentApplicationImportFields.DestinationSchoolName);
        string? grade = Get(EnrolmentApplicationImportFields.Grade);
        string? courseList = Get(EnrolmentApplicationImportFields.Subjects);

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
            bool dateTimeOfBirth = DateTime.TryParse(
                dateOfBirth,
                CultureInfo.GetCultureInfo("en-AU"),
                DateTimeStyles.None,
                out DateTime parsed);

            if (dateTimeOfBirth)
                birthDate = DateOnly.FromDateTime(parsed);
            else
                return Result.Failure<Application>(ImportErrors.ValueParseError(typeof(DateOnly), "Date Of Birth"));
        }

        Gender gender = Gender.Empty;
        if (!string.IsNullOrWhiteSpace(genderString))
        {
            Gender? genderValue = Gender.FromValue(genderString);

            if (genderValue is null)
                return Result.Failure<Application>(ImportErrors.ValueParseError(typeof(Gender), "Gender"));

            gender = genderValue;
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

        Result<Application> application = Application.Create(
            period.Id,
            srn,
            studentNameResult.Value,
            gender,
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

        if (application.IsFailure || courseList is null)
            return application;
        
        string[] courses = courseList.Split(';');
        List<EnrolmentCourse> validCourses = EnrolmentCourse.GetOptions.ToList();
        List<EnrolmentCourse> selectedCourses = [];

        foreach (string course in courses)
        {
            EnrolmentCourse? foundCourse = validCourses.FirstOrDefault(entry =>
                entry.Value == course.Trim()
                || entry.Name == course.Trim());

            if (foundCourse is null)
                return Result.Failure<Application>(ImportErrors.ValueParseError(typeof(EnrolmentCourse), "Courses"));

            selectedCourses.Add(foundCourse);
        }

        foreach (EnrolmentCourse course in selectedCourses)
        {
            application.Value.AddCourse(course);
            //application.Value.UpdateCourse(course, CourseSelectionStatus.Approved);
        }

        return application;
    }

    public async Task<Result> ApplyUpdates(
        Application existing, 
        StagedImportRow row,
        IReadOnlyDictionary<string, string?> columnMapping,
        EnrolmentPeriod context,
        CancellationToken cancellationToken = default)
    {
        bool IsMapped(string key) => ImportRowValueAccessor.IsMapped(columnMapping, key);
        string? Get(string key) => ImportRowValueAccessor.Get(row, columnMapping, key);

        if (IsMapped(EnrolmentApplicationImportFields.StudentNameFirst) || IsMapped(EnrolmentApplicationImportFields.StudentNameLast) || IsMapped(EnrolmentApplicationImportFields.StudentNamePreferred))
        {
            if (!IsMapped(EnrolmentApplicationImportFields.StudentNameFirst) || !IsMapped(EnrolmentApplicationImportFields.StudentNameLast))
                return Result.Failure(ImportErrors.IncompleteFieldGroup("Student Name"));

            Result<Name> nameResult = Name.Create(
                Get(EnrolmentApplicationImportFields.StudentNameFirst), Get(EnrolmentApplicationImportFields.StudentNamePreferred), Get(EnrolmentApplicationImportFields.StudentNameLast));

            if (nameResult.IsFailure)
                return nameResult;

            Result updateResult = existing.UpdateStudentName(nameResult.Value);
            if (updateResult.IsFailure)
                return updateResult;
        }

        if (IsMapped(EnrolmentApplicationImportFields.Grade))
        {
            bool gradeResult = Enum.TryParse(Get(EnrolmentApplicationImportFields.Grade), out Grade gradeValue);

            if (!gradeResult)
                return Result.Failure(ImportErrors.ValueParseError(typeof(Grade), "Grade"));

            Result updateResult = existing.UpdateGrade(gradeValue);
            if (updateResult.IsFailure)
                return updateResult;
        }

        if (IsMapped(EnrolmentApplicationImportFields.StudentReferenceNumber))
        {
            string? srn = Get(EnrolmentApplicationImportFields.StudentReferenceNumber);

            if (!string.IsNullOrWhiteSpace(srn))
            {
                Result<StudentReferenceNumber> srnResult = StudentReferenceNumber.Create(Get(EnrolmentApplicationImportFields.StudentReferenceNumber));

                if (srnResult.IsFailure)
                    return srnResult;

                Result updateResult = existing.UpdateSRN(srnResult.Value);
                if (updateResult.IsFailure)
                    return updateResult;
            }
        }

        if (IsMapped(EnrolmentApplicationImportFields.ParentNameFirst) || IsMapped(EnrolmentApplicationImportFields.ParentNameLast))
        {
            if (!IsMapped(EnrolmentApplicationImportFields.ParentNameFirst) || !IsMapped(EnrolmentApplicationImportFields.ParentNameLast))
                return Result.Failure(ImportErrors.IncompleteFieldGroup("Parent Name"));

            Result<Name> nameResult = Name.Create(
                Get(EnrolmentApplicationImportFields.ParentNameFirst), string.Empty, Get(EnrolmentApplicationImportFields.ParentNameLast));

            if (nameResult.IsFailure)
                return nameResult;

            Result updateResult = existing.UpdateParentName(nameResult.Value);
            if (updateResult.IsFailure)
                return updateResult;
        }

        if (IsMapped(EnrolmentApplicationImportFields.DateOfBirth))
        {
            string? dateString = Get(EnrolmentApplicationImportFields.DateOfBirth);

            if (!string.IsNullOrWhiteSpace(dateString))
            {
                bool dateTimeOfBirth = DateTime.TryParse(
                    dateString,
                    CultureInfo.GetCultureInfo("en-AU"),
                    DateTimeStyles.None,
                    out DateTime parsed);

                if (!dateTimeOfBirth)
                    return Result.Failure(ImportErrors.ValueParseError(typeof(DateOnly), "Date Of Birth"));

                Result updateResult = existing.UpdateDateOfBirth(DateOnly.FromDateTime(parsed));
                if (updateResult.IsFailure)
                    return updateResult;
            }
        }

        if (IsMapped(EnrolmentApplicationImportFields.Gender))
        {
            string? gender = Get(EnrolmentApplicationImportFields.Gender);

            if (!string.IsNullOrWhiteSpace(gender))
            {
                Result<Gender> genderResult = Gender.FromValue(Get(EnrolmentApplicationImportFields.Gender));

                if (genderResult.IsFailure)
                    return genderResult;

                Result updateResult = existing.UpdateGender(genderResult.Value);
                if (updateResult.IsFailure)
                    return updateResult;
            }
        }

        if (IsMapped(EnrolmentApplicationImportFields.StudentEmailAddress))
        {
            string? email = Get(EnrolmentApplicationImportFields.StudentEmailAddress);

            if (!string.IsNullOrWhiteSpace(email))
            {
                Result<EmailAddress> studentEmailAddress = EmailAddress.Create(email);

                if (studentEmailAddress.IsFailure)
                    return studentEmailAddress;

                Result updateResult = existing.UpdateStudentEmail(studentEmailAddress.Value);
                if (updateResult.IsFailure)
                    return updateResult;
            }
        }

        if (IsMapped(EnrolmentApplicationImportFields.ParentEmailAddress))
        {
            string? email = Get(EnrolmentApplicationImportFields.ParentEmailAddress);

            if (!string.IsNullOrWhiteSpace(email))
            {
                Result<EmailAddress> parentEmailAddress = EmailAddress.Create(email);

                if (parentEmailAddress.IsFailure)
                    return parentEmailAddress;

                Result updateResult = existing.UpdateParentEmail(parentEmailAddress.Value);
                if (updateResult.IsFailure)
                    return updateResult;
            }
        }

        if (IsMapped(EnrolmentApplicationImportFields.ParentPhoneNumber))
        {
            string? phoneNumber = Get(EnrolmentApplicationImportFields.ParentPhoneNumber);

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                Result<PhoneNumber> parentPhone = PhoneNumber.Create(phoneNumber);

                if (parentPhone.IsFailure)
                    return parentPhone;

                Result updateResult = existing.UpdateParentPhone(parentPhone.Value);
                if (updateResult.IsFailure)
                    return updateResult;
            }
        }

        if (IsMapped(EnrolmentApplicationImportFields.MailingAddressStreet)
            || IsMapped(EnrolmentApplicationImportFields.MailingAddressTown)
            || IsMapped(EnrolmentApplicationImportFields.MailingAddressState)
            || IsMapped(EnrolmentApplicationImportFields.MailingAddressPostcode))
        {
            if (!IsMapped(EnrolmentApplicationImportFields.MailingAddressStreet)
                || !IsMapped(EnrolmentApplicationImportFields.MailingAddressTown)
                || !IsMapped(EnrolmentApplicationImportFields.MailingAddressState)
                || !IsMapped(EnrolmentApplicationImportFields.MailingAddressPostcode))
                return Result.Failure(ImportErrors.IncompleteFieldGroup("Mailing Address"));

            string? street = Get(EnrolmentApplicationImportFields.MailingAddressStreet);
            string? town = Get(EnrolmentApplicationImportFields.MailingAddressTown);
            string? state = Get(EnrolmentApplicationImportFields.MailingAddressState);
            string? postcode = Get(EnrolmentApplicationImportFields.MailingAddressPostcode);

            if (string.IsNullOrWhiteSpace(street)
                || string.IsNullOrWhiteSpace(town)
                || string.IsNullOrWhiteSpace(state)
                || string.IsNullOrWhiteSpace(postcode))
                return Result.Failure(ImportErrors.IncompleteFieldGroup("Mailing Address"));

            Result<MailingAddress> mailingAddress = MailingAddress.Create(
                street, town, state, postcode);

            if (mailingAddress.IsFailure)
                return mailingAddress;

            Result updateResult = existing.UpdateMailingAddress(mailingAddress.Value);
            if (updateResult.IsFailure)
                return updateResult;
        }

        if (IsMapped(EnrolmentApplicationImportFields.CurrentSchoolName))
        {
            string? currentSchool = Get(EnrolmentApplicationImportFields.CurrentSchoolName);
            SchoolCode? schoolCode = null;

            if (!string.IsNullOrWhiteSpace(currentSchool))
            {
                School? foundSchool = await _schoolRepository.GetByName(currentSchool, cancellationToken);

                schoolCode = foundSchool is null
                    ? null
                    : foundSchool.Code;
            }

            Result updateResult = existing.UpdateCurrentSchool(schoolCode, currentSchool);
            if (updateResult.IsFailure)
                return updateResult;
        }

        if (IsMapped(EnrolmentApplicationImportFields.DestinationSchoolName))
        {
            string? destinationSchool = Get(EnrolmentApplicationImportFields.DestinationSchoolName);

            if (destinationSchool is null)
                return Result.Failure(ImportErrors.ValueParseError(typeof(School), "Destination School"));

            School? foundSchool = await _schoolRepository.GetByName(destinationSchool, cancellationToken);

            if (foundSchool is null)
                return Result.Failure(ImportErrors.ValueParseError(typeof(School), "Destination School"));

            Result updateResult = existing.UpdateDestinationSchool(foundSchool.Code, foundSchool.Name);
            if (updateResult.IsFailure)
                return updateResult;
        }

        if (IsMapped(EnrolmentApplicationImportFields.ApplicationReference))
        {
            string? applicationReference = Get(EnrolmentApplicationImportFields.ApplicationReference);

            if (!string.IsNullOrWhiteSpace(applicationReference))
            {
                Result updateResult = existing.UpdateApplicationReference(applicationReference);
                if (updateResult.IsFailure)
                    return updateResult;
            }
        }

        if (IsMapped(EnrolmentApplicationImportFields.Subjects))
        {
            string? courseList = Get(EnrolmentApplicationImportFields.Subjects);
            string[] courses = courseList?.Split(';') ?? [];
            List<EnrolmentCourse> validCourses = EnrolmentCourse.GetOptions.ToList();
            List<EnrolmentCourse> selectedCourses = [];

            foreach (string course in courses)
            {
                EnrolmentCourse? foundCourse = validCourses.FirstOrDefault(entry =>
                    entry.Value == course.Trim()
                    || entry.Name == course.Trim());

                if (foundCourse is null)
                    return Result.Failure<Application>(ImportErrors.ValueParseError(typeof(EnrolmentCourse), "Courses"));

                selectedCourses.Add(foundCourse);
            }

            foreach (EnrolmentCourse course in selectedCourses)
            {
                CourseSelection? existingCourse = existing.SelectedCourses
                    .FirstOrDefault(entry => entry.Course == course);

                if (existingCourse is null)
                    existing.AddCourse(course);
            }

            foreach (CourseSelection course in existing.SelectedCourses)
            {
                if (selectedCourses.All(entry => entry != course.Course))
                    existing.RemoveCourse(course.Course);
            }
        }

        return Result.Success();
    }
}
