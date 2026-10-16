namespace Constellation.Infrastructure.Services;

using Application.Common.Errors;
using Application.Domains.EnrolmentContext.Applications.Models;
using Application.Domains.Import.Helpers;
using Application.Domains.Import.Interfaces;
using Application.Domains.Import.Models;
using Application.Interfaces.Repositories;
using Application.Models.ImportCache;
using Core.Enums;
using Core.Errors;
using Core.Models;
using Core.Models.EnrolmentContext.Application;
using Core.Models.EnrolmentContext.Application.Enums;
using Core.Models.EnrolmentContext.EnrolmentParent;
using Core.Models.EnrolmentContext.EnrolmentStudent;
using Core.Models.Identifiers;
using Core.Models.Students.Enums;
using Core.Models.Students.ValueObjects;
using Core.Shared;
using Core.ValueObjects;
using System.Collections.Generic;
using System.Globalization;

internal sealed class ApplicationImportRowMapper : IImportRowMapper<(Application Application, EnrolmentStudent Student), ApplicationImportContext>
{
    private readonly ISchoolRepository _schoolRepository;

    public ApplicationImportRowMapper(
        ISchoolRepository schoolRepository)
    {
        _schoolRepository = schoolRepository;
    }

    public async Task<Result<(Application Application, EnrolmentStudent Student)>> MapNew(
        StagedImportRow row,
        IReadOnlyDictionary<string, string?> columnMapping,
        ApplicationImportContext context,
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
            return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.RequiredFieldMissing("Student First Name", row.RowNumber));

        if (string.IsNullOrWhiteSpace(studentNameLast))
            return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.RequiredFieldMissing("Student Last Name", row.RowNumber));

        if (string.IsNullOrWhiteSpace(grade))
            return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.RequiredFieldMissing("Grade", row.RowNumber));

        Result<Name> studentNameResult = Name.Create(
            studentNameFirst,
            studentNamePreferred ?? string.Empty,
            studentNameLast);

        if (studentNameResult.IsFailure)
            return Result.Failure<(Application, EnrolmentStudent)>(studentNameResult.Error);

        bool gradeResult = Grade.TryParse(grade, out Grade gradeValue);

        if (!gradeResult)
            return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.ValueParseError(typeof(Grade), "Grade"));

        StudentReferenceNumber? srn = null;
        if (!string.IsNullOrWhiteSpace(studentReferenceNumber))
        {
            Result<StudentReferenceNumber> srnResult = StudentReferenceNumber.Create(studentReferenceNumber);

            if (srnResult.IsFailure)
                return Result.Failure<(Application, EnrolmentStudent)>(srnResult.Error);

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
                return Result.Failure<(Application, EnrolmentStudent)>(parentNameResult.Error);

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
                return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.ValueParseError(typeof(DateOnly), "Date Of Birth"));
        }

        Gender gender = Gender.Empty;
        if (!string.IsNullOrWhiteSpace(genderString))
        {
            Gender? genderValue = Gender.FromValue(genderString);

            if (genderValue is null)
                return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.ValueParseError(typeof(Gender), "Gender"));

            gender = genderValue;
        }

        EmailAddress? studentEmail = null;
        if (!string.IsNullOrWhiteSpace(studentEmailAddress))
        {
            Result<EmailAddress> studentEmailAddressResult = EmailAddress.Create(studentEmailAddress);

            if (studentEmailAddressResult.IsFailure)
                return Result.Failure<(Application, EnrolmentStudent)>(studentEmailAddressResult.Error);

            studentEmail = studentEmailAddressResult.Value;
        }

        EnrolmentStudent student = EnrolmentStudent.Create(
            studentNameResult.Value,
            gender,
            srn,
            birthDate,
            studentEmail);

        EmailAddress? parentEmail = null;
        if (!string.IsNullOrWhiteSpace(parentEmailAddress))
        {
            Result<EmailAddress> parentEmailAddressResult = EmailAddress.Create(parentEmailAddress);

            if (parentEmailAddressResult.IsFailure)
                return Result.Failure<(Application, EnrolmentStudent)>(parentEmailAddressResult.Error);

            parentEmail = parentEmailAddressResult.Value;
        }

        PhoneNumber? parentPhone = null;
        if (!string.IsNullOrWhiteSpace(parentPhoneNumber))
        {
            Result<PhoneNumber> parentPhoneNumberResult = PhoneNumber.Create(parentPhoneNumber);

            if (parentPhoneNumberResult.IsFailure)
                return Result.Failure<(Application, EnrolmentStudent)>(parentPhoneNumberResult.Error);

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
                return Result.Failure<(Application, EnrolmentStudent)>(mailingAddressResult.Error);

            address = mailingAddressResult.Value;
        }

        EnrolmentParent parent = EnrolmentParent.Create(
            null,
            parentName,
            parentEmail,
            parentPhone,
            address);

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
                return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.ValueParseError(typeof(School), "Destination School"));

            destinationSchoolCode = foundSchool.Code;
            destinationSchool = foundSchool.Name;
        }

        Result<Application> application = Application.Create(
            context.Period.Id,
            student.Id,
            parent,
            applicationReference,
            currentSchoolCode,
            currentSchool,
            destinationSchoolCode,
            destinationSchool,
            context.Period.Program,
            gradeValue);

        if (application.IsFailure)
            return Result.Failure<(Application, EnrolmentStudent)>(application.Error);

        if (string.IsNullOrWhiteSpace(courseList))
            return (application.Value, student);

        Result<List<(EnrolmentCourse Course, string Line)>> parsedCourses = ParseCourses(courseList);

        if (parsedCourses.IsFailure)
            return Result.Failure<(Application, EnrolmentStudent)>(parsedCourses.Error);

        foreach ((EnrolmentCourse Course, string Line) course in parsedCourses.Value)
        {
            IEnumerable<EnrolmentCourse> validCourses = context.Period.AvailableCourses.Any()
                ? context.Period.AvailableCourses
                : EnrolmentCourse.GetOptions;

            EnrolmentCourse? foundCourse = validCourses.FirstOrDefault(entry => entry == course.Course);

            if (foundCourse is null)
                return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.ValueParseError(typeof(EnrolmentCourse), "Courses"));

            application.Value.AddCourse(course.Course, course.Line);
        }

        return (application.Value, student);
    }

    public async Task<Result<(Application Application, EnrolmentStudent Student)>> MapNewWithExistingStudent(
        StagedImportRow row,
        IReadOnlyDictionary<string, string?> columnMapping,
        ApplicationImportContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.ExistingStudent is null)
            return Result.Failure<(Application, EnrolmentStudent)>(ApplicationErrors.ArgumentNull(nameof(EnrolmentStudent)));

        bool IsMapped(string key) => ImportRowValueAccessor.IsMapped(columnMapping, key);
        string? Get(string fieldKey) => ImportRowValueAccessor.Get(row, columnMapping, fieldKey);

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

        if (string.IsNullOrWhiteSpace(grade))
            return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.RequiredFieldMissing("Grade", row.RowNumber));

        bool gradeResult = Grade.TryParse(grade, out Grade gradeValue);

        if (!gradeResult)
            return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.ValueParseError(typeof(Grade), "Grade"));

        Name? parentName = null;
        if (!string.IsNullOrWhiteSpace(parentNameFirst) && !string.IsNullOrWhiteSpace(parentNameLast))
        {
            Result<Name> parentNameResult = Name.Create(
                parentNameFirst,
                string.Empty,
                parentNameLast);

            if (parentNameResult.IsFailure)
                return Result.Failure<(Application, EnrolmentStudent)>(parentNameResult.Error);

            parentName = parentNameResult.Value;
        }

        EmailAddress? parentEmail = null;
        if (!string.IsNullOrWhiteSpace(parentEmailAddress))
        {
            Result<EmailAddress> parentEmailAddressResult = EmailAddress.Create(parentEmailAddress);

            if (parentEmailAddressResult.IsFailure)
                return Result.Failure<(Application, EnrolmentStudent)>(parentEmailAddressResult.Error);

            parentEmail = parentEmailAddressResult.Value;
        }

        PhoneNumber? parentPhone = null;
        if (!string.IsNullOrWhiteSpace(parentPhoneNumber))
        {
            Result<PhoneNumber> parentPhoneNumberResult = PhoneNumber.Create(parentPhoneNumber);

            if (parentPhoneNumberResult.IsFailure)
                return Result.Failure<(Application, EnrolmentStudent)>(parentPhoneNumberResult.Error);

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
                return Result.Failure<(Application, EnrolmentStudent)>(mailingAddressResult.Error);

            address = mailingAddressResult.Value;
        }

        EnrolmentParent? parent = parentName is null 
            ? null 
            : context.ExistingStudent.Parents.FirstOrDefault(entry => entry.Name == parentName);

        if (parent is null)
        {
            parent = EnrolmentParent.Create(
                null,
                parentName,
                parentEmail,
                parentPhone,
                address);
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
                return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.ValueParseError(typeof(School), "Destination School"));

            destinationSchoolCode = foundSchool.Code;
            destinationSchool = foundSchool.Name;
        }

        Result<Application> application = Application.Create(
            context.Period.Id,
            context.ExistingStudent.Id,
            parent,
            applicationReference,
            currentSchoolCode,
            currentSchool,
            destinationSchoolCode,
            destinationSchool,
            context.Period.Program,
            gradeValue);

        if (application.IsFailure)
            return Result.Failure<(Application, EnrolmentStudent)>(application.Error);

        if (IsMapped(EnrolmentApplicationImportFields.StudentNameFirst)
            || IsMapped(EnrolmentApplicationImportFields.StudentNameLast)
            || IsMapped(EnrolmentApplicationImportFields.StudentNamePreferred)
            || IsMapped(EnrolmentApplicationImportFields.StudentReferenceNumber)
            || IsMapped(EnrolmentApplicationImportFields.DateOfBirth)
            || IsMapped(EnrolmentApplicationImportFields.Gender)
            || IsMapped(EnrolmentApplicationImportFields.StudentEmailAddress))
        {
            if (IsMapped(EnrolmentApplicationImportFields.StudentNameFirst) || IsMapped(EnrolmentApplicationImportFields.StudentNameLast) || IsMapped(EnrolmentApplicationImportFields.StudentNamePreferred))
            {
                if (!IsMapped(EnrolmentApplicationImportFields.StudentNameFirst) || !IsMapped(EnrolmentApplicationImportFields.StudentNameLast))
                    return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.IncompleteFieldGroup("Student Name"));

                Result<Name> nameResult = Name.Create(
                    Get(EnrolmentApplicationImportFields.StudentNameFirst), Get(EnrolmentApplicationImportFields.StudentNamePreferred) ?? string.Empty, Get(EnrolmentApplicationImportFields.StudentNameLast));

                if (nameResult.IsFailure)
                    return Result.Failure<(Application, EnrolmentStudent)>(nameResult.Error);

                context.ExistingStudent.UpdateStudentName(nameResult.Value);
            }

            if (IsMapped(EnrolmentApplicationImportFields.StudentReferenceNumber))
            {
                string? srn = Get(EnrolmentApplicationImportFields.StudentReferenceNumber);

                if (!string.IsNullOrWhiteSpace(srn))
                {
                    Result<StudentReferenceNumber> srnResult = StudentReferenceNumber.Create(Get(EnrolmentApplicationImportFields.StudentReferenceNumber));

                    if (srnResult.IsFailure)
                        return Result.Failure<(Application, EnrolmentStudent)>(srnResult.Error);

                    context.ExistingStudent.UpdateStudentReferenceNumber(srnResult.Value);
                }
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
                        return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.ValueParseError(typeof(DateOnly), "Date Of Birth"));

                    context.ExistingStudent.UpdateDateOfBirth(DateOnly.FromDateTime(parsed));
                }
            }

            if (IsMapped(EnrolmentApplicationImportFields.Gender))
            {
                string? gender = Get(EnrolmentApplicationImportFields.Gender);

                if (!string.IsNullOrWhiteSpace(gender))
                {
                    Result<Gender> genderResult = Gender.FromValue(Get(EnrolmentApplicationImportFields.Gender));

                    if (genderResult.IsFailure)
                        return Result.Failure<(Application, EnrolmentStudent)>(genderResult.Error);

                    context.ExistingStudent.UpdateGender(genderResult.Value);
                }
            }

            if (IsMapped(EnrolmentApplicationImportFields.StudentEmailAddress))
            {
                string? email = Get(EnrolmentApplicationImportFields.StudentEmailAddress);

                if (!string.IsNullOrWhiteSpace(email))
                {
                    Result<EmailAddress> studentEmailAddress = EmailAddress.Create(email);

                    if (studentEmailAddress.IsFailure)
                        return Result.Failure<(Application, EnrolmentStudent)>(studentEmailAddress.Error);

                    context.ExistingStudent.UpdateStudentEmail(studentEmailAddress.Value);
                }
            }
        }
        
        if (string.IsNullOrWhiteSpace(courseList))
            return (application.Value, context.ExistingStudent);

        Result<List<(EnrolmentCourse Course, string Line)>> parsedCourses = ParseCourses(courseList);

        if (parsedCourses.IsFailure)
            return Result.Failure<(Application, EnrolmentStudent)>(parsedCourses.Error);

        foreach ((EnrolmentCourse Course, string Line) course in parsedCourses.Value)
        {
            IEnumerable<EnrolmentCourse> validCourses = context.Period.AvailableCourses.Any()
                ? context.Period.AvailableCourses
                : EnrolmentCourse.GetOptions;

            EnrolmentCourse? foundCourse = validCourses.FirstOrDefault(entry => entry == course.Course);

            if (foundCourse is null)
                return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.ValueParseError(typeof(EnrolmentCourse), "Courses"));

            application.Value.AddCourse(course.Course, course.Line);
        }
        
        return (application.Value, context.ExistingStudent);
    }
    
    public async Task<Result> ApplyUpdates(
        (Application Application, EnrolmentStudent Student) existing, 
        StagedImportRow row,
        IReadOnlyDictionary<string, string?> columnMapping,
        ApplicationImportContext context,
        CancellationToken cancellationToken = default)
    {
        if (context.ExistingStudent is null)
            return Result.Failure(ApplicationErrors.ArgumentNull(nameof(EnrolmentStudent)));

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

            context.ExistingStudent.UpdateStudentName(nameResult.Value);
        }

        if (IsMapped(EnrolmentApplicationImportFields.Grade))
        {
            bool gradeResult = Grade.TryParse(Get(EnrolmentApplicationImportFields.Grade), out Grade gradeValue);

            if (!gradeResult)
                return Result.Failure(ImportErrors.ValueParseError(typeof(Grade), "Grade"));

            Result updateResult = existing.Application.UpdateGrade(gradeValue);
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

                context.ExistingStudent.UpdateStudentReferenceNumber(srnResult.Value);
            }
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

                context.ExistingStudent.UpdateDateOfBirth(DateOnly.FromDateTime(parsed));
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

                context.ExistingStudent.UpdateGender(genderResult.Value);
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

                context.ExistingStudent.UpdateStudentEmail(studentEmailAddress.Value);
            }
        }

        EnrolmentParent? parent = null;

        if (IsMapped(EnrolmentApplicationImportFields.ParentNameFirst) || IsMapped(EnrolmentApplicationImportFields.ParentNameLast))
        {
            if (!IsMapped(EnrolmentApplicationImportFields.ParentNameFirst) || !IsMapped(EnrolmentApplicationImportFields.ParentNameLast))
                return Result.Failure(ImportErrors.IncompleteFieldGroup("Parent Name"));

            Result<Name> nameResult = Name.Create(
                Get(EnrolmentApplicationImportFields.ParentNameFirst), string.Empty, Get(EnrolmentApplicationImportFields.ParentNameLast));

            if (nameResult.IsFailure)
                return nameResult;

            parent = existing.Application.Parents.FirstOrDefault(entry => entry.Name == nameResult.Value);

            if (parent is null)
            {
                parent = EnrolmentParent.Create(null, nameResult.Value, null, null, null);
                existing.Application.AddParent(parent);
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

                if (parent is not null)
                    parent.UpdateParentEmail(parentEmailAddress.Value);
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

                if (parent is not null)
                    parent.UpdateParentPhone(parentPhone.Value);
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

            if (parent is not null)
                parent.UpdateMailingAddress(mailingAddress.Value);
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

            Result updateResult = existing.Application.UpdateCurrentSchool(schoolCode, currentSchool);
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

            Result updateResult = existing.Application.UpdateDestinationSchool(foundSchool.Code, foundSchool.Name);
            if (updateResult.IsFailure)
                return updateResult;
        }

        if (IsMapped(EnrolmentApplicationImportFields.ApplicationReference))
        {
            string? applicationReference = Get(EnrolmentApplicationImportFields.ApplicationReference);

            if (!string.IsNullOrWhiteSpace(applicationReference))
            {
                Result updateResult = existing.Application.UpdateApplicationReference(applicationReference);
                if (updateResult.IsFailure)
                    return updateResult;
            }
        }

        if (IsMapped(EnrolmentApplicationImportFields.Subjects))
        {
            string? courseList = Get(EnrolmentApplicationImportFields.Subjects);

            Result<List<(EnrolmentCourse Course, string Line)>> parsedCourses = ParseCourses(courseList);

            if (parsedCourses.IsFailure)
                return Result.Failure<(Application, EnrolmentStudent)>(parsedCourses.Error);

            foreach ((EnrolmentCourse Course, string Line) course in parsedCourses.Value)
            {
                IEnumerable<EnrolmentCourse> validCourses = context.Period.AvailableCourses.Any()
                    ? context.Period.AvailableCourses
                    : EnrolmentCourse.GetOptions;

                EnrolmentCourse? foundCourse = validCourses.FirstOrDefault(entry => entry == course.Course);

                if (foundCourse is null)
                    return Result.Failure<(Application, EnrolmentStudent)>(ImportErrors.ValueParseError(typeof(EnrolmentCourse), "Courses"));

                CourseSelection? existingCourse = existing.Application.SelectedCourses
                    .FirstOrDefault(entry => entry.Course == foundCourse);

                if (existingCourse is null)
                    existing.Application.AddCourse(foundCourse, course.Line);
                else
                    existing.Application.UpdateCourse(existingCourse.Course, course.Line, existingCourse.Status);
            }
        }

        return Result.Success();
    }

    private static Result<List<(EnrolmentCourse Course, string Line)>> ParseCourses(string? courseList)
    {
        List<(EnrolmentCourse, string)> parsed = [];

        if (string.IsNullOrWhiteSpace(courseList))
            return Result.Success(parsed);

        foreach (string entry in courseList.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            string[] parts = entry.Split(':', 2, StringSplitOptions.TrimEntries);

            if (parts.Length != 2 || parts[1].Length == 0)
                return Result.Failure<List<(EnrolmentCourse, string)>>(ImportErrors.ValueParseError(typeof(EnrolmentCourse), "Courses"));

            EnrolmentCourse? course = EnrolmentCourse.GetOptions
                .FirstOrDefault(o => o.Value == parts[0] || o.Name == parts[0]);

            if (course is null)
                return Result.Failure<List<(EnrolmentCourse, string)>>(ImportErrors.ValueParseError(typeof(EnrolmentCourse), "Courses"));

            parsed.Add((course, parts[1]));
        }

        return Result.Success(parsed);
    }
}

