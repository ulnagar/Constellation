namespace Constellation.Core.Models.Assets.Errors;

using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class AssetErrors
{
    public static class AssetNumber
    {
        public static Error Empty = new(
            "Assets.AssetNumber.Empty",
            "AssetNumber must not be empty");

        public static Error TooLong = new(
            "Assets.AssetNumber.TooLong",
            "AssetNumber must be no larger than 8 numbers");

        public static Error UnknownCharacters = new(
            "Assets.AssetNumber.UnknownCharacters",
            "AssetNumber must be numbers only");
    }
}
