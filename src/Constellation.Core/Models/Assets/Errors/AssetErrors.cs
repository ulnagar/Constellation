namespace Constellation.Core.Models.Assets.Errors;

using Shared;

public static class AssetErrors
{
    public static class Asset
    {
        public static readonly Error SerialNumberEmpty = new(
            "Assets.Asset.SerialNumberEmpty",
            "Serial Number cannot be empty");
    }

    public static class AssetNumber
    {
        public static readonly Error Empty = new(
            "Assets.AssetNumber.Empty",
            "AssetNumber must not be empty");

        public static readonly Error TooLong = new(
            "Assets.AssetNumber.TooLong",
            "AssetNumber must be no larger than 8 numbers");

        public static readonly Error UnknownCharacters = new(
            "Assets.AssetNumber.UnknownCharacters",
            "AssetNumber must be numbers only");
    }

    public static class Allocation
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


        public static class CurrentLocation
        {
            public static readonly Error ReactivationBlocked = new(
                "Assets.Allocation.CurrentLocation.ReactivationBlocked",
                "Cannot reactivate an old location");
        }
    }
}
