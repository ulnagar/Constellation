using Constellation.Core.Models.Identifiers;
using Constellation.Core.Shared;
using System;

namespace Constellation.Core.Errors;
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

    public static class Assignments
    {
        public static class Assignment
        {
            public static readonly Func<AssignmentId, Error> NotFound = id => new(
                "Assignments.Assignment.NotFound",
                $"Could not find an assignment with the Id {id}");

            public static readonly Func<int, Error> NotFoundByCourse = id => new(
                "Assignments.Assignment.NotFoundByCourse",
                $"Could not find any assignments linked to the course with id {id}");
        }

        public static class Submission
        {
            public static readonly Func<AssignmentSubmissionId, Error> NotFound = id => new(
                "Assignments.Submission.NotFound",
                $"Could not find any submission with the id {id}");
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
        public static class Completion
        {
            public static readonly Error AlreadyExists = new(
                "MandatoryTraining.Completion.AlreadyExists",
                "A completion record with these details already exists");

            public static readonly Func<TrainingCompletionId, Error> NotFound = id => new(
                "MandatoryTraining.Completion.NotFound",
                $"A training completion record with the Id {id.Value} could not be found");
        }

        public static class Import
        {
            public static readonly Error NoDataFound = new(
                "MandatoryTraining.Import.NoDataFound",
                "Could not find any data in the import file");
        }

        public static class Module
        {
            public static readonly Func<TrainingModuleId, Error> NotFound = id => new Error(
                "MandatoryTraining.Module.NotFound",
                $"A training module with the Id {id.Value} could not be found");
        }
    }

    public static class Partners
    {
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
        }

        public static class Student
        {
            public static class AbsenceConfiguration
            {
                public static readonly Error AlreadyCancelled = new(
                    "Partners.Student.AbsenceConfiguration.AlreadyCancelled",
                    "This absence configuration has already been marked cancelled");

                public static readonly Func<DateOnly, DateOnly, Error> RecordForRangeExists = (startDate, endDate) => new(
                    "Partners.Student.AbsenceConfiguration.RecordForRangeExists",
                    $"A current configuration exists that covers some or all of the dates from {startDate} to {endDate}");
            }

            public static readonly Error InvalidId = new(
                "Partners.Student.InvalidId",
                "The provided student id is not valid");

            public static readonly Func<string, Error> NotFound = id => new Error(
                "Partners.Student.NotFound",
                $"A student with the Id {id} could not be found");

            public static readonly Func<string, Error> NotFoundForSchool = id => new Error(
                "Partners.Student.NotFoundForSchool",
                $"No current students found linked to school with Id {id}");
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

            public static readonly Func<SciencePracRollId, Error> NotFound = id => new(
                "SciencePracs.Rolls.NotFound",
                $"Could not find a roll with the Id {id}");

            public static readonly Error MustBeCancelled = new(
                "SciencePracs.Rolls.MustBeCancelled",
                "Cannot reinstate roll that has not been cancelled");
        }
    }

    public static class Subjects
    {
        public static class Course
        {
            public static readonly Func<int, Error> NoOfferings = id => new(
                "Subjects.Course.NoOfferings",
                $"Could not find any offerings related to course with id {id}");

            public static readonly Func<int, Error> NotFound = id => new(
                "Subjects.Course.NotFound",
                $"Could not find a course with the id {id}");
        }

        public static class Offering
        {
            public static readonly Func<int, Error> NotFound = id => new(
                "Subjects.Offering.NotFound",
                $"Could not find an offering with the id {id}");
        }
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
