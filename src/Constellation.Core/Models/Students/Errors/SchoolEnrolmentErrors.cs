namespace Constellation.Core.Models.Students.Errors;

using Constellation.Core.Shared;

public static class SchoolEnrolmentErrors
{
    public static readonly Error AlreadyExists = new(
        "Student.SchoolEnrolment.AlreadyExists",
        "This student already has a School Enrolment with these details");

    public static readonly Error NotFound = new(
        "Student.SchoolEnrolment.NotFound",
        "Could not find a current School Enrolment for this student");

    public static readonly Error TooMany = new(
        "Student.SchoolEnrolment.TooMany",
        "Found too many current School Enrolments for this student");

    public static readonly Error RecordDeleted = new(
        "Student.SchoolEnrolment.AlreadyDeleted",
        "This School Enrolment record has been marked deleted");
}