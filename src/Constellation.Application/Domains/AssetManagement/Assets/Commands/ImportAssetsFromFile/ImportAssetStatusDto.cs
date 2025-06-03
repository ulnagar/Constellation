#nullable enable
namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.ImportAssetsFromFile;

using Constellation.Core.Shared;

public sealed record ImportAssetStatusDto(
    int RowNumber,
    bool Succeeded,
    Error? Error);