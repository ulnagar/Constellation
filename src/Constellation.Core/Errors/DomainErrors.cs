namespace Constellation.Core.Errors;

using Constellation.Core.Enums;
using Constellation.Core.Models.Identifiers;
using Constellation.Core.Models.Offerings.Identifiers;
using Constellation.Core.Shared;
using System;

public static class DomainErrors
{
    public static class Absences
    {
        public static class Absence
        {
            public static readonly Error AlreadyExplained = new(
                "Absences.Absence.AlreadyExplained",
                "Cannot explain an absence that has already been explained");

            public static readonly Func<AbsenceId, Error> NotFound = id => new Error(
                "Absences.Absence.NotFound",
                $"Could not find any absence with the id {id}");
        }

        public static class Notification
        {
            public static readonly Func<AbsenceNotificationId, Error> NotFound = id => new(
                "Absences.Notification.NotFound",
                $"Could not find any absence notification with the Id {id}");
        }

        public static class Report
        {
            public static readonly Error NoFilterSupplied = new(
                "Absences.Report.NoFilterSupplied",
                "Cannot generate report without a supplied filter");
        }

        public static class Response
        {
            public static readonly Func<AbsenceResponseId, Error> NotFound = id => new Error(
                "Absences.Response.NotFound",
                $"Could not find any response with the id {id}");
        }
    }

    public static class Assets
    {
        public static class Allocations
        {
            public static readonly Func<string, Error> NotFoundForStudent = id => new Error(
                "Assets.Allocations.NotFoundForStudent",
                $"Could not find any asset allocations for student {id}");
        }
    }

    public static class Auth
    {
        public static readonly Error UserNotFound = new Error(
            "Auth.UserNotFound",
            "Cannot find a user that matches the entry");

        public static readonly Error RoleNotFound = new Error(
            "Auth.RoleNotFound",
            "Cannot find a role that matches the entry");

        public static readonly Func<string, Error> CannotUpdateRole = (role) => new Error(
            "Auth.CannotUpdateRole",
            $"Cannot update role {role}");

        public static readonly Error NotAuthorised = new(
            "Auth.NotAuthorised",
            "The current user is not authorised to complete this action");
    }

    public static class Awards
    {
        public static class NominationPeriod
        {
            public static readonly Func<AwardNominationPeriodId, Error> NotFound = id => new(
                "Awards.NominationPeriod.NotFound",
                $"Could not find a nomination period with the id {id}");

            public static readonly Error PastDate = new(
                "Awards.NominationPeriod.PastDate",
                "The specified Lockout Date for the Nomination Period is in the past and invalid");
        }

        public static class Nomination
        {
            public static readonly Error NotRecognised = new(
                "Awards.Nomination.NotRecognised",
                "Could not identify award type being nominated");

            public static readonly Func<AwardNominationId, Error> NotFound = id => new(
                "Awards.Nomination.NotFound",
                $"Could not find a nomination with the id {id}");
        }
    }

    public static class Casuals
    {
        public static class Casual
        {
            public static readonly Func<CasualId, Error> NotFound = id => new Error(
                "Casuals.Casual.NotFound",
                $"A Casual with the Id {id.Value} could not be found");
        }
    }

    public static class ClassCovers
    {
        public static class Cover
        {
            public static readonly Func<ClassCoverId, Error> NotFound = id => new Error(
                "ClassCovers.Cover.NotFound",
                $"A Class Cover with the Id {id.Value} could not be found");
        }
    }

    public static class Documents
    {
        public static class AwardCertificate
        {
            public static readonly Func<StudentAwardId, Error> NotFound = id => new(
                "Documents.AwardCertificate.NotFound",
                $"Could not find a document with the link id {id.Value}");
        }

        public static class AssignmentSubmission
        {
            public static readonly Func<string, Error> NotFound = id => new(
                "Documents.AssignmentSubmission.NotFound",
                $"Could not find a document with the link id {id}");
        }

        public static class AcademicReport
        {
            public static readonly Func<string, Error> NotFound = id => new(
                "Documents.AcademicReport.NotFound",
                $"Could not find a document with the link id {id}");
        }

        public static class TrainingCertificate
        {
            public static readonly Error NotFound = new(
                "Documents.TrainingCertificate.NotFound",
                "Could not find certificate in the database");
        }
    }

    public static class Enrolments
    {
        public static class Enrolment
        {
            public static readonly Func<string, Error> NotFoundForStudent = id => new Error(
                "Enrolments.Enrolment.NotFoundForStudent",
                $"No enrolments could be found for student with Id {id}");

            public static readonly Func<string, OfferingId, Error> AlreadyExists = (studentId, offeringId) => new(
                "Enrolments.Enrolment.AlreadyExists",
                $"A current enrolment already exists for student {studentId} and offering {offeringId}");
        }
    }

    public static class Families
    {
        public static class Family
        {
            public static readonly Error EmailAlreadyInUse = new(
                "Families.Family.EmailAlreadyInUse",
                "Email address is already linked to another family");

            public static readonly Func<FamilyId, Error> NotFound = id => new(
                "Families.Family.NotFound",
                $"Could not find a family with Id {id}");

            public static readonly Error InvalidAddress = new(
                "Families.Address.InvalidAddress",
                "The Address supplied is incomplete or invalid");
        }

        public static class Parents
        {
            public static readonly Func<ParentId, FamilyId, Error> NotFoundInFamily = (parentId, familyId) => new(
                "Families.Parent.NotFoundInFamily",
                $"Could not find a parent with Id {parentId} in the family with Id {familyId}");

            public static readonly Error AlreadyExists = new(
                "Families.Parent.AlreadyExists",
                "Cannot create a new parent as another parent already exists with these details");
        }

        public static class Students
        {
            public static readonly Error NoLinkedFamilies = new(
                "Families.Students.NoLinkedFamilies",
                "The student does not have any linked families in the database");

            public static readonly Error NoResidentialFamily = new(
                "Families.Students.NoResidentialFamily",
                "The student does not have any linked family marked as the residential family");
        }
    }

    public static class GroupTutorials
    {
        public static class GroupTutorial
        {
            public static readonly Error TutorialHasExpired = new(
                "GroupTutorials.GroupTutorial.TutorialHasExpired",
                "The Tutorial has already ended or has been deleted");

            public static readonly Func<GroupTutorialId, Error> NotFound = id => new Error(
                "GroupTutorials.GroupTutorial.NotFound",
                $"A tutorial with the Id {id.Value} could not be found");

            public static readonly Error CouldNotCreateTutorial = new(
                "GroupTutorials.GroupTutorial.CouldNotCreate",
                "There was an error attempting to create the group tutorial");
        }

        public static class TutorialTeacher
        {
            public static readonly Error NotFound = new(
                "GroupTutorials.TutorialTeacher.EntryNotFound",
                "There is no corresponding teacher record for that tutorial");
        }

        public static class TutorialEnrolment
        {
            public static readonly Error NotFound = new(
                "GroupTutorials.TutorialEnrolment.EntryNotFound",
                "There is no corresponding enrolment record for that tutorial");

            public static readonly Error StudentAlreadyEnrolled = new(
                "GroupTutorials.TutorialEnrolment.StudentAlreadyEnrolled",
                "The student is already actively enrolled in this tutorial");
        }

        public static class TutorialRoll
        {
            public static readonly Func<DateOnly, Error> RollAlreadyExistsForDate = rollDate => new Error(
                "GroupTutorials.TutorialRoll.RollAlreadyExistsForDate",
                $"A roll for date {rollDate.ToShortDateString()} already exists");

            public static readonly Func<DateOnly, Error> RollDateInvalid = rollDate => new Error(
                "GroupTutorials.TutorialRoll.RollDateInvalid",
                $"Cannot create a roll for {rollDate.ToShortDateString()} as this is not a valid date for this tutorial");

            public static readonly Func<TutorialRollId, Error> NotFound = id => new Error(
                "GroupTutorials.TutorialRoll.NotFound",
                $"A roll with the Id {id.Value} could not be found");

            public static readonly Error SubmitInvalidStatus = new Error(
                "GroupTutorials.TutorialRoll.SubmitInvalidStatus",
                "Cannot submit a roll that has been cancelled or previously submitted");

            public static readonly Func<string, Error> StudentNotFound = student => new Error(
                "GroupTutorials.TutorialRoll.StudentNotFound",
                $"Cannot find an attendance record for student with Id {student} attached to the roll");

            public static readonly Func<string, Error> RemoveEnrolledStudent = student => new Error(
                "GroupTutorials.TutorialRoll.RemoveEnrolledStudent",
                $"Cannot remove student with Id {student} from the roll as they are enrolled in the tutorial");
        }

    }

    public static class LinkedSystems
    {
        public static class AdobeConnect
        {
            public static readonly Func<string, Error> RoomNotFound = id => new(
                "LinkedSystems.AdobeConnect.RoomNotFound",
                $"Could not find an Adobe Connect Room with the Id {id}");

            public static readonly Error CannotCreate = new(
                "LinkedSystems.AdobeConnect.CannotCreate",
                "An error occured while waiting for Adobe Connect to create the room");
        }

        public static class Sentral
        {
            public static readonly Func<string, Error> FamilyIdNotValid = id => new Error(
                "LinkedSystems.Sentral.FamilyIdNotValid",
                $"The value {id} is not a valid Sentral Family Id.");
        }

        public static class Teams
        {
            public static readonly Error TeamNotFoundInDatabase = new(
                "LinkedSystems.Teams.TeamNotFoundInDatabase",
                "The Team could not be found in the database");

            public static readonly Func<Guid, Error> AlreadyExists = id => new Error(
                "LinkedSystems.Teams.AlreadyExists",
                $"The Team with Id {id} could not be created because it already exists in the database");
        }
    }

    public static class MandatoryTraining
    {

    }

    public static class Partners
    {
        public static class Contact
        {
            public static readonly Func<int, Error> NotFound = id => new(
                "Partners.Contact.NotFound",
                $"Could not find a School Contact with the Id {id}");
        }

        public static class Faculty
        {
            public static readonly Func<Guid, Error> NotFound = id => new(
                "Partners.Faculty.NotFound",
                $"Could not find a Faculty with the Id {id}");
        }

        public static class School
        {
            public static readonly Func<string, Error> NotFound = id => new(
                "Partners.School.NotFound",
                $"A school with the code {id} could not be found");
        }

        public static class Staff
        {
            public static readonly Func<int, Error> NotFoundLinkedToOffering = id => new Error(
                "Partners.Staff.NotFoundLinkedToOffering",
                $"Could not retrieve list of teachers for the offering {id}");

            public static readonly Func<string, Error> NotFound = id => new Error(
                "Partners.Staff.TeacherNotFound",
                $"A teacher with the Id {id} could not be found");

            public static readonly Func<string, Error> NotFoundByEmail = email => new Error(
                "Partners.Staff.TeacherNotFound",
                $"A teacher with the Email Address {email} could not be found");

            public static readonly Error NoneFound = new(
                "Partners.Staff.NoneFound",
                "Could not find any active staff in the database");
        }
    }

    public static class Reports
    {
        public static class AcademicReport
        {
            public static readonly Func<string, Error> NotFoundByPublishId = id => new(
                "Reports.AcademicReport.NotFoundByPublishId",
                $"An academic report with the publish Id of {id} could not be found");
        }
    }

    public static class SciencePracs
    {
        public static class Lesson
        {
            public static readonly Func<DateOnly, Error> PastDueDate = date => new(
                "SciencePracs.Lesson.PastDueDate",
                $"Cannot create a Lesson due in the past ({date})");

            public static readonly Error EmptyName = new(
                "SciencePracs.Lesson.EmptyName",
                "Cannot create a Lesson with a blank name");

            public static readonly Func<SciencePracLessonId, Error> NotFound = id => new(
                "SciencePracs.Lesson.NotFound",
                $"Could not find a lesson with the Id {id}");

            public static readonly Error RollCompleted = new(
                "SciencePracs.Lesson.RollCompleted",
                "Cannot cancel a lesson that has completed rolls");

            public static readonly Error CannotEdit = new(
                "SciencePracs.Lesson.CannotEdit",
                "Cannot edit a lesson after a roll has been submitted");
        }

        public static class Roll
        {
            public static readonly Error AlreadyExistsForSchool = new(
                "SciencePracs.Roll.AlreadyExistsForSchool",
                "A Roll already exists in this Lesson for this school");

            public static readonly Error CommentRequiredNonePresent = new(
                "SciencePracs.Rolls.CommentRequiredNonePresent",
                "Cannot mark a roll with no students present without providing a comment");

            public static readonly Error CannotCancelCompletedRoll = new(
                "SciencePracs.Rolls.CannotCancelCompletedRolls",
                "Cannot mark roll cancelled if it has already been submitted as marked");

            public static readonly Error CannotReinstateRoll = new(
                "SciencePracs.Rolls.CannotReinstateRoll",
                "Cannot reinstate a roll that has not been cancelled");

            public static readonly Func<SciencePracRollId, Error> NotFound = id => new(
                "SciencePracs.Rolls.NotFound",
                $"Could not find a roll with the Id {id}");

            public static readonly Error MustBeCancelled = new(
                "SciencePracs.Rolls.MustBeCancelled",
                "Cannot reinstate roll that has not been cancelled");
        }
    }

    public static class Period
    {
        public static readonly Error NoneFoundForOffering = new(
            "Periods.Period.NoneFoundForOffering",
            "Could not find Periods attached to Offering");
    }

    public static class Permissions
    {
        public static readonly Error Unauthorised = new Error(
            "Permissions.Unauthorised",
            "You do not have the required permissions to perform this action");
    }

    public static class ValueObjects
    {
        public static class EmailAddress
        {
            public static readonly Error EmailEmpty = new Error(
                "ValueObjects.EmailAddress.EmailEmpty",
                "Email Address must not be empty.");

            public static readonly Error EmailInvalid = new Error(
                "ValueObjects.EmailAddress.EmailInvalid",
                "Email Address is not valid.");
        }

        public static class EmailRecipient
        {
            public static readonly Error NameEmpty = new Error(
                "ValueObjects.EmailRecipient.NameEmpty",
                "Email Recipient must have a valid name.");
        }

        public static class Name
        {
            public static readonly Error FirstNameEmpty = new Error(
                "ValueObjects.Name.FirstNameEmpty",
                "First Name must not be empty.");

            public static readonly Error LastNameEmpty = new Error(
                "ValueObjects.Name.LastNameEmpty",
                "Last Name must not be empty.");
        }

        public static class OfferingName
        {
            public static readonly Func<Grade, Error> InvalidGrade = grade => new(
                "ValueObjects.OfferingName.InvalidGrade",
                $"Invalid grade supplied: {grade.ToString()}");

            public static readonly Func<string, Error> InvalidCourseCode = code => new(
                "ValueObjects.OfferingName.InvalidCourseCode",
                $"Invalid course code supplied: {code}");

            public static readonly Func<string, Error> InvalidTutorialInitials = initials => new(
                "ValueObjects.OfferingName.InvalidTutorialInitials",
                $"Invalid initals supplied for tutorial class: {initials}");
        }

        public static class PhoneNumber
        {
            public static readonly Error NumberEmpty = new(
                "ValueObjects.PhoneNumber.NumberEmpty",
                "Phone Number must not be empty");

            public static readonly Error NumberInvalid = new(
                "ValueObjects.PhoneNumber.NumberInvalid",
                "Phone NUmber is not valid");
        }
    }
}
