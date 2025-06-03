namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.AllocateAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;

public sealed record AllocateAssetToSchoolCommand(
    AssetNumber AssetNumber,
    string SchoolCode)
    : ICommand;