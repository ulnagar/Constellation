namespace Constellation.Core.Extensions;

using LocationCategory = Models.Stocktake.Enums.LocationCategory;

public static class AssetLocationCategoryExtensions
{
    public static LocationCategory AsStocktakeLocationCategory(this Models.Assets.Enums.LocationCategory assetCategory) =>
        assetCategory switch
        {
            _ when assetCategory.Equals(Models.Assets.Enums.LocationCategory.CoordinatingOffice) => LocationCategory
                .AuroraCollege,
            _ when assetCategory.Equals(Models.Assets.Enums.LocationCategory.CorporateOffice) => LocationCategory
                .StateOffice,
            _ when assetCategory.Equals(Models.Assets.Enums.LocationCategory.PublicSchool) => LocationCategory
                .PublicSchool,
            _ when assetCategory.Equals(Models.Assets.Enums.LocationCategory.PrivateResidence) => LocationCategory
                .PrivateResidence,
            _ => LocationCategory.Other
        };
}