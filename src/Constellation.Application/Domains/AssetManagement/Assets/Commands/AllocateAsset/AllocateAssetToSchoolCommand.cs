namespace Constellation.Application.Domains.AssetManagement.Assets.Commands.AllocateAsset;

using Constellation.Application.Abstractions.Messaging;
using Constellation.Core.Models.Assets.ValueObjects;
using Core.Models.Identifiers;

public sealed record AllocateAssetToSchoolCommand(
    AssetNumber AssetNumber,
    SchoolCode SchoolCode)
    : ICommand;