namespace Constellation.Application.Assets.AllocateAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;

public sealed record AllocateAssetToSchoolCommand(
    AssetNumber AssetNumber,
    string SchoolCode)
    : ICommand;