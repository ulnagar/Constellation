namespace Constellation.Application.Assets.AllocateAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;

public sealed record AllocateAssetToStaffMemberCommand(
    AssetNumber AssetNumber,
    string StaffId)
    : ICommand;
