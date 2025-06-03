namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.AllocateAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;
using Constellation.Core.Models.StaffMembers.Identifiers;

public sealed record AllocateAssetToStaffMemberCommand(
    AssetNumber AssetNumber,
    StaffId StaffId)
    : ICommand;
