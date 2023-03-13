namespace Constellation.Application.ExternalDataConsistency;

using Constellation.Application.Abstractions.Messaging;
using System.Collections.Generic;
using System.IO;

public sealed record MasterFileConsistencyCoordinator(
    MemoryStream MasterFileStream,
    bool EmailReport,
    string EmailAddress = default)
    : ICommand<List<UpdateItem>>;