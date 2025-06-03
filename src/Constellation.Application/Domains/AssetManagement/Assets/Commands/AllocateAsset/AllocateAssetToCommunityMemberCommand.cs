namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.AllocateAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;

public sealed record AllocateAssetToCommunityMemberCommand(
    AssetNumber AssetNumber,
    string UserName,
    string UserEmail)
    : ICommand;
