namespace Constellation.Application.Assets.ImportAssetsFromFile;

using Abstractions.Messaging;
using System.IO;

public sealed record ImportAssetsFromFileCommand(
    MemoryStream ImportFile)
    : ICommand;