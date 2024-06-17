namespace Constellation.Application.Assets.AddAssetNote;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;

public sealed record AddAssetNoteCommand(
    AssetNumber AssetNumber,
    string Note)
    : ICommand;