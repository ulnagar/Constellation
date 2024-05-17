namespace Constellation.Core.Models.Assets.Errors;

using Shared;

public static class AllocationErrors
{
    public static readonly Error StudentEmpty = new(
        "Assets.Allocation.StudentEmpty",
        "A Student record is required to create an Allocation for a student");

    public static readonly Error StaffEmpty = new(
        "Assets.Allocation.StaffEmpty",
        "A Staff record is required to create an Allocation for a staff member");

    public static readonly Error SchoolEmpty = new(
        "Assets.Allocation.SchoolEmpty",
        "A School record is required to create an Allocation for a school");

    public static readonly Error RecipientEmpty = new(
        "Assets.Allocation.RecipientEmpty",
        "A contact record is required to create an Allocation for a community member");
}