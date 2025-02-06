namespace Constellation.Core.Models.Families.Errors;

using Shared;

public static class FamilyStudentErrors
{
    public static readonly Error NoLinkedFamilies = new(
        "Families.Students.NoLinkedFamilies",
        "The student does not have any linked families in the database");

    public static readonly Error NoResidentialFamily = new(
        "Families.Students.NoResidentialFamily",
        "The student does not have any linked family marked as the residential family");
}
