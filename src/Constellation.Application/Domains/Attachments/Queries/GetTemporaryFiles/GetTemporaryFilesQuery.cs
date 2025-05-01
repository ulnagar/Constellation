namespace Constellation.Application.Domains.Attachments.Queries.GetTemporaryFiles;

using Abstractions.Messaging;
using Models;
using System.Collections.Generic;

public sealed record GetTemporaryFilesQuery()
    : IQuery<List<ExternalReportTemporaryFileResponse>>;