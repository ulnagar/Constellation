namespace Constellation.Application.Assets.AllocateAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;

public sealed record AllocateAssetToCommunityMemberCommand(
    AssetNumber AssetNumber,
    string UserName,
    string UserEmail)
    : ICommand;
