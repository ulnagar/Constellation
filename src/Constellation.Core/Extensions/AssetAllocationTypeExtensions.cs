namespace Constellation.Core.Extensions;

using Models.Assets.Enums;
using Models.Stocktake.Enums;

public static class AssetAllocationTypeExtensions
{
    public static UserType AsStocktakeUserType(this AllocationType allocationType) =>
        allocationType switch
        {
            _ when allocationType.Equals(AllocationType.Student) => UserType.Student,
            _ when allocationType.Equals(AllocationType.Staff) => UserType.Staff,
            _ when allocationType.Equals(AllocationType.School) => UserType.School,
            _ when allocationType.Equals(AllocationType.CommunityMember) => UserType.CommunityMember,
            _ => UserType.Other
        };
}