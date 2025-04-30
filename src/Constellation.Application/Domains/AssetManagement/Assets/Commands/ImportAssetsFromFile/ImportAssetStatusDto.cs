#nullable enable
namespace Constellation.Application.Assets.ImportAssetsFromFile;

using Core.Shared;

public sealed record ImportAssetStatusDto(
    int RowNumber,
    bool Succeeded,
    Error? Error);