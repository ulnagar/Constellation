#nullable enable
namespace Constellation.Infrastructure.Persistence.ConstellationContext.Converters;

using Core.Models.Assets.ValueObjects;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

internal sealed class AssetNumberConverter : ValueConverter<AssetNumber, string?>
{
    public AssetNumberConverter()
        : base(
            asset => AssetNumberToString(asset),
            value => StringToAssetNumber(value),
            new ConverterMappingHints()) 
    { }

    private static string? AssetNumberToString(AssetNumber? number) =>
        number is null ? null
        : number == AssetNumber.Empty ? null
        : number.ToString();

    private static AssetNumber StringToAssetNumber(string? value)
    {
        if (value is null)
            return AssetNumber.Empty;
                
        AssetNumber? assetNumber = AssetNumber.FromValue(value);

        return assetNumber ?? AssetNumber.Empty;
    }

    public override bool ConvertsNulls => true;
}