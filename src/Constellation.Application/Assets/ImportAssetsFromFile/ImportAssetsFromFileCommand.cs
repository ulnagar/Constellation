namespace Constellation.Application.Assets.ImportAssetsFromFile;

using Abstractions.Messaging;
using Core.Shared;
using System.Collections.Generic;
using System.IO;

public sealed record ImportAssetsFromFileCommand(
    MemoryStream ImportFile)
    : ICommand<List<Error>>;