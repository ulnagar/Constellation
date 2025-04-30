namespace Constellation.Application.Assets.AllocateAsset;

using Abstractions.Messaging;
using Core.Models.Assets.ValueObjects;
using Core.Models.Students.Identifiers;

public sealed record AllocateAssetToStudentCommand(
    AssetNumber AssetNumber,
    StudentId StudentId)
    : ICommand;