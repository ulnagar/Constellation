namespace Constellation.Application.Assets.AllocateAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;

public sealed record AllocateAssetToStudentCommand(
    AssetNumber AssetNumber,
    string StudentId)
    : ICommand;